using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using log4net;
using Microsoft.Azure;
using Newtonsoft.Json;
using Paycor.Import;
using Paycor.Import.Messaging;
using Paycor.Import.Web;
using Paycor.Import.Status;
using Paycor.Import.Shared;
using Paycor.Import.Employee.Status;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Paycor.Import.Azure;
using Paycor.Import.Http;
using Paycor.Import.ImportHistory;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace EmployeeImportWorker
{
    public class EmployeeImportExporter : RestApiExporter
    {

        #region API payload domain classes

        [ExcludeFromCodeCoverage]
        public class ValidationTotals
        {
            public int? TotalRecordsImported { get; set; }
            public int? TotalProcessed { get; set; }
            public int? TotalRecords { get; set; }
        }

        [ExcludeFromCodeCoverage]
        public class ValidationReport
        {
            public long EmployeeNumber { get; set; }
            public string LastName { get; set; }
            public string Description { get; set; }
            public string MessageType { get; set; }
        }

        [ExcludeFromCodeCoverage]
        public class EEImportResultContent
        {
            public IEnumerable<ValidationTotals> ValidationTotals { get; set; }
            public IEnumerable<ValidationReport> ValidationReport { get; set; }
            public int EmployeeUpdateCount { get; set; }
            public bool HasDuplicateEmployees { get; set; }
        }

        [ExcludeFromCodeCoverage]
        protected class Client
        {
            public int Id { get; set; }
            public string FEIN { get; set; }
            public string Name { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class EEImportPayload
        {
            public string ClientId { get; set; }
            public EmployeeImportStatusEnum Status { get; set; }
            public IEnumerable<IDictionary<string, string>> ImportData { get; set; }
        }


        [ExcludeFromCodeCoverage]
        private class EEImportValidationErrorResponse
        {
            public string Message { get; set; }
            public Dictionary<string, string[]> ModelState { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class EEImportValidationDetail
        {
            public int RecordIndex { get; set; }
            public string[] ColumnValidationErrors { get; set; }
        }

        #endregion

        #region ReadOnly

        public const string BaseAddressKey = "PerformBaseUri";
        public const string EmployeeImportApiEndpointKey = "EmployeeImportApiEndpoint";
        public const string ChunkSizeKey = "ChunkSize";
        public const string CompanyDomainBaseAddressKey = "CompanyDomainBaseUri";
        public const string CompanyDomainApiEndpointKey = "CompanyDomainApiEndpoint";
        private const int DefaultChunkSize = 1000;

        private readonly ILog _log;
        private readonly IImportHistoryService _importHistoryService;
        private readonly int _httpTimeout;
        private readonly INotificationMessageClient _notificationMessageClient;
        private string _importDetailUri;

        private int _totalRecords;

        public readonly List<EmployeeImportStatusEnum> StatusOrder = new List<EmployeeImportStatusEnum>
        {
            EmployeeImportStatusEnum.ClearImportTables,
            EmployeeImportStatusEnum.InitiateEmployeeImport,
            EmployeeImportStatusEnum.ImportFileData,
            EmployeeImportStatusEnum.ValidateRecordTypes,
            EmployeeImportStatusEnum.ValidateDataTypes,
            EmployeeImportStatusEnum.ValidateGeneralRecord,
            EmployeeImportStatusEnum.ValidateStaticRecord,
            EmployeeImportStatusEnum.ValidateEarningRecord,
            EmployeeImportStatusEnum.ValidateDeductionRecord,
            EmployeeImportStatusEnum.ValidateBenefitRecord,
            EmployeeImportStatusEnum.ValidateDirectDepositRecord,
            EmployeeImportStatusEnum.ValidateRateRecord,
            EmployeeImportStatusEnum.ValidateTaxRecord,
            EmployeeImportStatusEnum.RetrieveValidationResults,
            EmployeeImportStatusEnum.EmployeeImportUpdateGeneral,
            EmployeeImportStatusEnum.EmployeeImportUpdateGroupTermLifeEarnings,
            EmployeeImportStatusEnum.EmployeeImportUpdateEarnings,
            EmployeeImportStatusEnum.EmployeeImportUpdateDefaultEarnings,
            EmployeeImportStatusEnum.EmployeeImportUpdateDeductions,
            EmployeeImportStatusEnum.EmployeeImportUpdateTaxes,
            EmployeeImportStatusEnum.EmployeeImportUpdateDirectDeposit,
            EmployeeImportStatusEnum.EmployeeImportUpdateRates,
            EmployeeImportStatusEnum.EmployeeImportUpdateBenefits,
        };

        #endregion

        #region Fields

        private string _baseAddress = string.Empty;
        private string _apiEndpoint = string.Empty;
        private string _companyDomainBaseAddress = string.Empty;
        private string _companyDomainApiEndpoint = string.Empty;
        private int _chunkSize = DefaultChunkSize;
        private Guid _masterSessionId;
        private readonly IStatusStorageProvider _storageProvider;
        private readonly string _container;
        private EmployeeImportStatusMessage _statusMessage;

        #endregion

        #region Public methods

        public EmployeeImportExporter(ILog log, IStatusStorageProvider storageProvider,
            IImportHistoryService importHistoryService, int timeout,
            INotificationMessageClient notificationMessageClient)
            : base(log)
        {
            _container = ContainerNames.ImportStatus;

            Ensure.ThatArgumentIsNotNull(storageProvider, nameof(storageProvider));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(_container, nameof(_container));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            Ensure.ThatArgumentIsNotNull(notificationMessageClient, nameof(notificationMessageClient));

            _storageProvider = storageProvider;
            _log = log;
            _importHistoryService = importHistoryService;
            _httpTimeout = timeout;
            _notificationMessageClient = notificationMessageClient;
        }

        public override bool Initialize(NameValueCollection configSettings)
        {
            _baseAddress = (null == configSettings)
                ? CloudConfigurationManager.GetSetting(BaseAddressKey)
                : configSettings[BaseAddressKey];

            var result = !string.IsNullOrEmpty(_baseAddress);

            _companyDomainBaseAddress = (null == configSettings)
                ? CloudConfigurationManager.GetSetting(CompanyDomainBaseAddressKey)
                : configSettings[CompanyDomainBaseAddressKey];

            result &= !string.IsNullOrEmpty(_companyDomainBaseAddress);

            _apiEndpoint = (null == configSettings)
                ? CloudConfigurationManager.GetSetting(EmployeeImportApiEndpointKey)
                : configSettings[EmployeeImportApiEndpointKey];

            result &= !string.IsNullOrEmpty(_apiEndpoint);

            _companyDomainApiEndpoint = (null == configSettings)
                ? CloudConfigurationManager.GetSetting(CompanyDomainApiEndpointKey)
                : configSettings[CompanyDomainApiEndpointKey];

            result &= !string.IsNullOrEmpty(_companyDomainApiEndpoint);

            var chunkSize = (configSettings == null)
                ? CloudConfigurationManager.GetSetting(ChunkSizeKey)
                : configSettings[ChunkSizeKey];

            if (int.TryParse(chunkSize, out _chunkSize))
            {
                _log.DebugFormat("PerformApi row upload chunk size set to {0}", _chunkSize);
            }
            else
            {
                _chunkSize = DefaultChunkSize;
            }

            if (result)
            {
                _log.DebugFormat(
                    "EmployeeImportExporter: BaseAddress = {0}, ApiEndpoint = {1}, CompanyDomain BaseAddress = {2}, CompanyDomain ApiEndpoint = {3} ",
                    _baseAddress, _apiEndpoint, _companyDomainBaseAddress, _companyDomainApiEndpoint);
            }
            else
            {
                _log.ErrorFormat(ImportResource.ImportMissingConfigurationSettings,
                    BaseAddressKey, EmployeeImportApiEndpointKey);
            }

            _importDetailUri = CloudConfigurationManager.GetSetting("ImportDetailUri");

            _log.Debug("EmployeeImport Exporter initialized");
            return (result);
        }

        #endregion

        #region Protected methods

        protected override async Task<HttpExporterResult> OnExportAsync(RestApiPayload restApiPayload)
        {
            // ** IMPORTANT **
            // Since this is the outer most function for the exporter, we want to put as much of the
            // processing inside the try block as possible, so that if an unexpected exception occurs,
            // appropriate feedback can be sent to the user and the exception details can be logged.

            // In order to communicate with the user in either the nominal or exception case, a Status Engine must be set up. 
            // While it may be possible for an exception to be thrown while setting it up, the risk should be minimal if
            // all remaining processing occurs within the try block below.
            var statusLogger = EmployeeImportStatus.GetStatusEngine(restApiPayload.TransactionId, _container,
                _storageProvider, _importHistoryService);
            _statusMessage = statusLogger.RetrieveMessage();
            _statusMessage.ImportType = ImportConstants.Employee;

            HttpExporterResult result = null;

            try
            {
                _log.Debug("Starting export.");
                var employeeImportRestApiPayload = restApiPayload as EeImportRestApiPayload;
                Ensure.ThatArgumentIsNotNull(employeeImportRestApiPayload, nameof(employeeImportRestApiPayload));

                // ReSharper disable once PossibleNullReferenceException - handled with Enforce
                employeeImportRestApiPayload.ApiEndpoint = _apiEndpoint;
                _masterSessionId = new Guid(employeeImportRestApiPayload.MasterSessionId);

                var firstRecord = employeeImportRestApiPayload.Records.FirstOrDefault();
                if (firstRecord == null)
                {
                    throw new ArgumentException(EmployeeImportResource.EEImportMissingClientId);
                }

                var clientId = firstRecord["ClientId"];

                var client = CallCompanyDomain(clientId);
                if (client == null)
                {
                    throw new ImportException(EmployeeImportResource.EEImportInvalidClientId);
                }

                _statusMessage.Total = ImportConstants.TotalSteps;
                _statusMessage.Current = 0;
                foreach (var status in StatusOrder)
                {
                    if (status == EmployeeImportStatusEnum.ImportFileData)
                    {
                        var totalChunks = ((employeeImportRestApiPayload.RecordCount - 1)/_chunkSize) + 1;
                        for (var count = 0; count < totalChunks; count++)
                        {
                            var fileData =
                                employeeImportRestApiPayload.Records.Skip(count*_chunkSize).Take(_chunkSize);
                            result = await CallPerformAsync(statusLogger, clientId, status,
                                employeeImportRestApiPayload.ApiEndpoint, employeeImportRestApiPayload.RecordCount,
                                fileData);
                        }
                    }
                    else
                    {
                        result = await CallPerformAsync(statusLogger, clientId, status, employeeImportRestApiPayload.ApiEndpoint,
                            employeeImportRestApiPayload.RecordCount);
                        if (CancelImportBasedOnMappingValue(employeeImportRestApiPayload, status))
                        {
                            _log.Info(
                                $"Cancelling import by request due to errors/warnings: {clientId}, status: {status}.");
                            break;
                        }
                    }
                    _statusMessage.PercentComplete = CalculatePercentage.GetPercentageComplete(
                        _statusMessage.Current, _statusMessage.Total);
                    _statusMessage.Current += 1;
                    _statusMessage.Status = status;
                    _statusMessage.ClientId = clientId;
                    statusLogger.LogMessage(_statusMessage);
                }
            }
            catch (BadRequestImportException)
            {
                // Do nothing. All of the error formatting has been handled in CallPerform().
            }
            catch (Exception e)
            {
                _log.Error("An exception occurred during the export process.", e);
                var totalFailedRecords = restApiPayload.RecordCount;
                _statusMessage.FailRecordsCount = totalFailedRecords;
                _statusMessage.SuccessRecordsCount = 0;
                var detailItem = new EmployeeImportStatusDetail
                {
                    EmployeeName = "All",
                    EmployeeNumber = "All",
                    Issue =
                        "There was a problem that prevented completion of the import in progress. Please download report for details.",
                    RecordUploaded = false,
                    IssueType = EmployeeImportIssueTypeEnum.Error.ToString()
                };
                var details = new List<EmployeeImportStatusDetail> {detailItem};
                _statusMessage.StatusDetails = details;
            }
            finally
            {
                _statusMessage.PercentComplete = ImportConstants.FullCompletedPercentage;
                _statusMessage.Total = ImportConstants.TotalSteps;
                _statusMessage.Status = EmployeeImportStatusEnum.ImportComplete;
                await statusLogger.LogMessageAsync(_statusMessage);

                var longMessage = ImportConstants.ProcessCompleteMsgLong.Replace("{{here}}", $"<a href='{_importDetailUri}/{restApiPayload.TransactionId}'>here</a>");

                _notificationMessageClient.Send(_masterSessionId, ApiKeyData.ApiKey, ApiKeyData.ApiSecretKey,
                   ImportConstants.ProcessCompleteMsgPurpose,
                   ImportConstants.ProcessCompleteMsgShort,
                   ImportConstants.ProcessCompleteMsgMedium,
                   longMessage,
                   NotificationTypes.ImportCompletion);
            }

            _log.Debug("Export complete.");
            return result;
        }

        protected virtual Client CallCompanyDomain(string clientId)
        {
            Client client;
            var apiUrl = GetCompanyDomainService(clientId);
            var httpclient = CreateHttpClient();

            _log.DebugFormat("Calling company domain endpoint {0} with clientId {1}.", _companyDomainApiEndpoint,
                clientId);

            var response = httpclient.GetAsync(apiUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                var responseString = responseContent.ReadAsStringAsync().Result;
                client = JsonConvert.DeserializeObject<Client>(responseString);
            }
            else
            {
                HandleResponseErrorCode(response);
                throw new Exception(EmployeeImportResource.CompanyDomainCallDidNotSucceed);
            }
            return client;
        }

        protected bool HasAnyValidationError()
        {
            return _statusMessage.StatusDetails.Any(message => message.RecordUploaded != null &&
                                                               !(bool) message.RecordUploaded &&
                                                               _statusMessage.FailRecordsCount > 0);
        }

        protected bool HasAnyValidationWarning()
        {
            return
                _statusMessage.StatusDetails.Any(
                    message => message.RecordUploaded != null && (bool) message.RecordUploaded);
        }

        protected override async Task<HttpExporterResult> PostToApiAsync(string jsonData, string apiEndpoint)
        {
            var result = new HttpExporterResult();
            //This try-catch is added due to async tasks in different threads
            try
            {
                var httpClient = CreateHttpClient();

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiEndpoint, stringContent);
                result.Response = response;
            }
            catch (AggregateException ae)
            {
                var flat = ae.Flatten();
                _log.Error(flat.Message, flat);
                foreach (var aggException in flat.InnerExceptions)
                {
                    _log.Error(aggException.Message, aggException);
                }
            }
            catch (Exception e)
            {
                result.Exception = e;
                _log.Error(e.Message, e);

                var realerror = e;
                while (realerror.InnerException != null)
                {
                    realerror = realerror.InnerException;
                    _log.Error(realerror.Message, realerror);
                }
            }
            return result;
        }

        [ExcludeFromCodeCoverage]
        protected override HttpClient CreateHttpClient()
        {
            var apiHttpClient = new ApiCookieHttpClient(_baseAddress, _masterSessionId, _httpTimeout);
            var httpClient = apiHttpClient.CreateHttpClient();

            return httpClient;
        }

        #endregion

        #region Private methods

        private bool CancelImportBasedOnMappingValue(RestApiPayload restApiPayload, EmployeeImportStatusEnum status)
        {
            var eeImportRestApiPayload = restApiPayload as EeImportRestApiPayload;
            if (eeImportRestApiPayload == null ||
                !eeImportRestApiPayload.MappingValue.Equals(
                    ImportConstants.EeImportMappingEnum.CancelImportOnErrorsOrWarnings.ToString(),
                    StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }
            if (status == EmployeeImportStatusEnum.RetrieveValidationResults && HasAnyValidationError()
                ||
                status == EmployeeImportStatusEnum.RetrieveValidationResults && HasAnyValidationWarning())
            {
                _statusMessage.SummaryErrorMessage =
                    "0 Record imported due to warnings and/or errors. If Continue option is selected - " +
                    $"{_statusMessage.SuccessRecordsCount} record(s) will be imported and " +
                    $"{_statusMessage.FailRecordsCount} record(s) will not be imported";

                if (HasAnyValidationWarning() || HasAnyValidationError())
                {
                    _statusMessage.SuccessRecordsCount = 0;
                    _statusMessage.FailRecordsCount = _totalRecords;
                }

                _statusMessage.PercentComplete = ImportConstants.FullCompletedPercentage;
                _statusMessage.Total = ImportConstants.TotalSteps;
                return true;
            }
            return false;
        }

        private string GetCompanyDomainService(string clientId)
        {
            return $"{_companyDomainBaseAddress}{_companyDomainApiEndpoint}/v1/clients/{clientId}";
        }

        private async Task<HttpExporterResult> CallPerformAsync(EmployeeImportStatusLogger statusLogger,
            string clientId,
            EmployeeImportStatusEnum status,
            string apiEndpoint,
            int payloadRecordCount,
            IEnumerable<IDictionary<string, string>> fileData = null)
        {
            var jsonPayload = new EEImportPayload
            {
                ClientId = clientId,
                Status = status
            };

            if (fileData != null)
            {
                jsonPayload.ImportData = fileData;
            }

            var jsonData = JsonConvert.SerializeObject(jsonPayload);
            _log.Info($"Perform called for client: {clientId}, status: {status}, endpoint: {apiEndpoint}.");

            var result = await PostToApiAsync(jsonData, apiEndpoint);
            if (null == result)
            {
                _log.Warn("HttpResult from PostToApi is null.");
                return null;
            }

            if (result.IsSuccess)
            {
                var resultContent = ExtractResultContent(result);
                if (null == resultContent)
                    return result;

                var returnData = ParseResultContent(resultContent);
                if (null == returnData)
                    return result;

                SaveStatusFromApi(returnData, status, statusLogger, payloadRecordCount);
            }
            else
            {
                var resultResponse = CheckResultResponse(result);
                if (resultResponse != null)
                {
                    _log.WarnFormat("Perform was call with status: {0} and a http response of {1} was returned.", status,
                        resultResponse.StatusCode);

                    var responseStatusCode = resultResponse.StatusCode;
                    _log.Debug(responseStatusCode.ToString());

                    if (responseStatusCode == HttpStatusCode.BadRequest)
                    {
                        var resultContent = ExtractResultContent(result);
                        if (null == resultContent)
                            return result;

                        var returnData = ParseValidationErrorResponse(resultContent);
                        if (null == returnData)
                            return result;

                        HandleApiValidationErrors(returnData, jsonPayload);
                        throw new BadRequestImportException();
                    }

                    HandleResponseErrorCode(resultResponse);
                    throw new Exception(_statusMessage.Message);
                }
            }
            return result;
        }

        private string ExtractResultContent(HttpExporterResult httpResult)
        {
            var resultResponse = CheckResultResponse(httpResult);
            if (null == resultResponse)
                return null;

            var responseContent = CheckResponseContent(resultResponse);
            if (null == responseContent)
                return null;

            return CheckContentResult(responseContent);
        }

        private HttpResponseMessage CheckResultResponse(HttpExporterResult result)
        {
            var resultResponse = result.Response;
            if (null == resultResponse)
                _log.Warn("Response in Result is not present.");

            return resultResponse;
        }

        private HttpContent CheckResponseContent(HttpResponseMessage resultResponse)
        {
            var responseContent = resultResponse.Content;
            if (null == responseContent)
                _log.Warn(ImportResource.ImportNoResponseContent);

            return responseContent;
        }

        private string CheckContentResult(HttpContent responseContent)
        {
            var resultContent = responseContent.ReadAsStringAsync().Result;
            if (null == resultContent)
                _log.Warn("Result response is not present in response content.");

            return resultContent;
        }

        private EEImportResultContent ParseResultContent(string resultContent)
        {
            var returnData = JsonConvert.DeserializeObject<EEImportResultContent>(resultContent);
            if (null == returnData)
                _log.Info("EEImportResultContent: Return data is not present after parsing JSON object.");

            return returnData;
        }

        private EEImportValidationErrorResponse ParseValidationErrorResponse(string resultContent)
        {
            var returnData = JsonConvert.DeserializeObject<EEImportValidationErrorResponse>(resultContent);
            if (null == returnData)
                _log.Info("EEImportValidationErrorResponse: Return data is not present after parsing JSON object.");

            return returnData;
        }

        private void HandleApiValidationErrors(EEImportValidationErrorResponse content, EEImportPayload jsonPayload)
        {
            var importData = jsonPayload.ImportData as IDictionary<string, string>[] ??
                             jsonPayload.ImportData.ToArray();

            if (!string.IsNullOrWhiteSpace(content.Message) && content.ModelState.Count > 0)
            {
                var lstValidationDetails = content.ModelState.Keys.ToArray()
                    .Select(item => new EEImportValidationDetail()
                    {
                        RecordIndex = ExtractRecordIndex(item),
                        ColumnValidationErrors = content.ModelState[item]
                    }).ToList();

                _log.DebugFormat("Total number of validation errors from api : {0}", lstValidationDetails.Count);

                var lstEmployeeImportStatusDetails = (from item in lstValidationDetails
                    select new EmployeeImportStatusDetail
                    {
                        EmployeeName = importData.ElementAt(item.RecordIndex)["LastName"],
                        EmployeeNumber = importData.ElementAt(item.RecordIndex)["EmployeeNumber"],
                        Issue = item.ColumnValidationErrors.FirstOrDefault(),
                        IssueType = EmployeeImportIssueTypeEnum.Error.ToString()
                    }).ToList();

                _statusMessage.StatusDetails = lstEmployeeImportStatusDetails;
            }
            _statusMessage.FailRecordsCount = importData.Length;
            _statusMessage.SuccessRecordsCount = 0;
        }

        private int ExtractRecordIndex(string item)
        {
            const string startingStr = "request.ImportData[";
            const string endingStr = "]";

            var startingPosition = GetStringPosition(item, startingStr);
            var endingPosition = GetStringPosition(item, endingStr);

            var adjustedStartingPosition = startingPosition + startingStr.Length;
            if (adjustedStartingPosition >= endingPosition)
                HandleValidationParseError();

            var index = item.Substring(adjustedStartingPosition, endingPosition - adjustedStartingPosition);
            var recordIndex = int.Parse(index);

            return recordIndex;
        }

        private int GetStringPosition(string item, string value)
        {
            var position = item.IndexOf(value, StringComparison.Ordinal);
            if (position == -1)
                HandleValidationParseError();

            return position;
        }

        private void HandleValidationParseError()
        {
            _log.Error(EmployeeImportResource.EEImportValidationParseError);
            throw new Exception(EmployeeImportResource.EEImportValidationParseError);
        }

        private void SaveStatusFromApi(EEImportResultContent content, EmployeeImportStatusEnum resultStatus,
            EmployeeImportStatusLogger statusLogger, int payloadRecordCount)
        {
            Ensure.ThatArgumentIsNotNull(statusLogger, nameof(statusLogger));
            Ensure.ThatArgumentIsNotNull(content, nameof(content));

            _statusMessage.Status = resultStatus;

            if (content.ValidationReport != null)
            {
                foreach (var reportItem in content.ValidationReport)
                {
                    if (reportItem != null)
                    {
                        var detail = new EmployeeImportStatusDetail
                        {
                            EmployeeName = reportItem.LastName,
                            EmployeeNumber = reportItem.EmployeeNumber.ToString(CultureInfo.InvariantCulture),
                            Issue = reportItem.Description,
                            RecordUploaded =
                                reportItem.MessageType != null &&
                                reportItem.MessageType.ToLower().Contains("informational"),
                            IssueType =(reportItem.MessageType != null &&
                                        reportItem.MessageType.ToLower().Contains("informational")) 
                                        ? EmployeeImportIssueTypeEnum.Warning.ToString() 
                                        : EmployeeImportIssueTypeEnum.Error.ToString()

                        };
                        _statusMessage.StatusDetails.Add(detail);
                    }
                }
            }

            if (content.ValidationTotals != null)
            {
                SetSuccessAndFailedCount(content.ValidationTotals, payloadRecordCount);
            }
            statusLogger.LogMessage(_statusMessage);
        }

        private void SetSuccessAndFailedCount(IEnumerable<ValidationTotals> contentValidationTotals,
            int payloadRecordCount)
        {
            Ensure.ThatArgumentIsNotNull(contentValidationTotals, nameof(contentValidationTotals));

            var totalImported =
                contentValidationTotals.Where(v => v != null && v.TotalRecordsImported != null)
                    .Sum(x => x.TotalRecordsImported) ?? 0;

            var totalRecords =
                contentValidationTotals.Where(v => v != null && v.TotalRecords != null)
                    .Sum(x => x.TotalRecords) ?? 0;

            totalRecords = totalRecords > payloadRecordCount ? payloadRecordCount : totalRecords;

            if (totalRecords >= totalImported)
            {
                _statusMessage.SuccessRecordsCount = totalImported;
                _statusMessage.FailRecordsCount = totalRecords - totalImported;
                _totalRecords = totalRecords;
            }
            else
            {
                _log.ErrorFormat(
                    "Total Records imported is greater than total records, Success Records Count - {0}, Failed Records Count - {1}",
                    _statusMessage.SuccessRecordsCount, _statusMessage.FailRecordsCount);
                _statusMessage.SuccessRecordsCount = 0;
                _statusMessage.FailRecordsCount = 0;
            }
        }

        private void HandleResponseErrorCode(HttpResponseMessage response)
        {
            var errorMessage = ErrorMessages.HttpResponseErrors(response);

            var responseContent = CheckResponseContent(response);
            if (null != responseContent)
            {
                var contentResult = CheckContentResult(responseContent);
                if (contentResult != null)
                {
                    _log.Error(contentResult);
                    errorMessage += ExtractExceptionError(contentResult);
                }
            }
            _statusMessage.Message = string.IsNullOrEmpty(errorMessage)
                ? ImportResource.ProcessingException
                : errorMessage;
        }

        private string ExtractExceptionError(string content)
        {
            var specificError = string.Empty;
            var stringSeparators = new[] {ImportConstants.CRLF};
            var errorLines = content.Split(stringSeparators, StringSplitOptions.None);
            if (errorLines.Length > 0)
            {
                if (errorLines[0].Contains(ImportConstants.Subquery))
                {
                    specificError = EmployeeImportResource.EEImportSubqueryException;
                }
                else
                {
                    const int arrNumber = 1;
                    const string errPrefix = ImportConstants.Issue;
                    var spaceSeparator = new[] {ImportConstants.Space};

                    var error = errorLines[0].Split(spaceSeparator, StringSplitOptions.None);                  
                    if (error.Length > 1)
                    {
                        specificError += errPrefix + error[arrNumber];
                    }
                }
            }
            return specificError;
        }

        #endregion
    }
}