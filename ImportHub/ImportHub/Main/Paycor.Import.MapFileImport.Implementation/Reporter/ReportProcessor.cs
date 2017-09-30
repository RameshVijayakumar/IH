using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.ImportHistory;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Messaging;
using Paycor.Import.Status;

namespace Paycor.Import.MapFileImport.Implementation.Reporter
{
    public class ReportProcessor : IReportProcessor
    {
        private readonly ICloudMessageClient<FileImportEventMessage> _eventMessageClient;
        private readonly string _serviceBusConnectionString;
        private readonly string _importDetailUri;
        private readonly ILog _log;
        private readonly IImportHistoryService _importHistoryService;
        private readonly IXlsxRecordFormatter<FailedRecord> _failedRecordFormatter;

        public ReportProcessor(
            ICloudMessageClient<FileImportEventMessage> eventMessageClient,
            ILog log, IImportHistoryService importHistoryService, IXlsxRecordFormatter<FailedRecord> failedRecordFormatter, IErrorFormatter errorFormatter)
        {
            Ensure.ThatArgumentIsNotNull(eventMessageClient, nameof(eventMessageClient));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            Ensure.ThatArgumentIsNotNull(failedRecordFormatter, nameof(failedRecordFormatter));
            Ensure.ThatArgumentIsNotNull(errorFormatter, nameof(errorFormatter));
            _eventMessageClient = eventMessageClient;
            _log = log;
            _importHistoryService = importHistoryService;
            _failedRecordFormatter = failedRecordFormatter;
            _serviceBusConnectionString = ConfigurationManager.AppSettings["PaycorServiceBusConnection"];
            _importDetailUri = ConfigurationManager.AppSettings["ImportDetailUri"];
            Ensure.ThatStringIsNotNullOrEmpty(_serviceBusConnectionString, nameof(_serviceBusConnectionString));
            Ensure.ThatStringIsNotNullOrEmpty(_importDetailUri, nameof(_importDetailUri));
        }

        public void SendEvent(MappedFileImportStatusMessage statusMessage)
        {
            var completionEvent = new FileImportEventMessage
            {
                ClientId = statusMessage.ClientId,
                Source = statusMessage.Source,
                TransactionId = statusMessage.Id,
                MapType = statusMessage.ImportType,
                TotalFailed = statusMessage.FailRecordsCount ?? 0,
                TotalImported = statusMessage.SuccessRecordsCount ?? 0,
                SummaryResult =
                    $"Success: {statusMessage.SuccessRecordsCount ?? 0}, Failed: {statusMessage.FailRecordsCount ?? 0}",
                SummaryUrl = $"{_importDetailUri}/{statusMessage.Id}"
            };

            try
            {
                _eventMessageClient.SendMessage(completionEvent, MappedFileTopicInfo.TopicName,
                    _serviceBusConnectionString);
                _log.Debug($"Completed sending completion {completionEvent.SummaryUrl} event at report processor.");
            }
            catch (Exception ex)
            {
                _log.Error(
                    $"Unable to send the completion message: {completionEvent} to topic: {MappedFileTopicInfo.TopicName}",
                    ex);
            }
        }

        public void SaveFailedData(string transactionId, List<FailedRecord> failedRecords, int clientId = 0)
        {
            if (!failedRecords.Any())
            {
                return;
            }
            var data = GenerateFailedData(failedRecords);
            if (data == null) return;
            using (var stream = new MemoryStream(data))
            {
                _importHistoryService.SaveFailedRecords(clientId, transactionId, stream);
            }
            _log.Debug("Completed SaveFailedData at report processor.");
        }

        public void SaveFailedData(string transactionId, IDictionary<string, IList<FailedRecord>> failedRecordDictionary, int clientId = 0)
        {
            if (!failedRecordDictionary.Values.Any())
            {
                return;
            }
            var data = GenerateFailedDataForMultiSheet(failedRecordDictionary);
            if (data == null) return;
            using (var stream = new MemoryStream(data))
            {
                _importHistoryService.SaveFailedRecords(clientId, transactionId, stream);
            }
        }

        public async Task SaveStatusFromApiAsync(MappedFileImportStatusMessage statusMessage, MappedFileImportStatusLogger statusLogger, IEnumerable<ErrorResultData> errorResultDataItems)
        {
            Ensure.ThatArgumentIsNotNull(statusLogger, nameof(statusLogger));
            try
            {
                InitializeRecordCount(statusMessage);
                if (errorResultDataItems != null)
                    SetStatus(statusMessage, errorResultDataItems);

                await statusLogger.LogMessageAsync(statusMessage);
            }
            catch (Exception ex)
            {
               _log.Error($"Exception occurred at {nameof(ReportProcessor)}:{nameof(SaveStatusFromApiAsync)}", ex);
            }  
        }

        private void SetStatus(MappedFileImportStatusMessage statusMessage, IEnumerable<ErrorResultData> errorResultDataItems)
        {

            foreach (var errorResultData in errorResultDataItems)
            {
                if (errorResultData == null)
                {
                    continue;
                }
                if ((errorResultData.HttpExporterResult == null) || !errorResultData.HttpExporterResult.IsSuccess)
                {
                    statusMessage.Status = MappedFileImportStatusEnum.ImportFileData;
                    AddErrorStatusDetails(statusMessage, errorResultData);
                }
                else
                {
                    var importLink = errorResultData.HttpExporterResult.GetLinkFromApi();
                    if (importLink != null)
                    {
                        statusMessage.ApiLink = importLink;
                    }
                }
            }
        }

        public async Task UpdateStatusRecordCountAsync(MappedFileImportStatusMessage statusMessage, MappedFileImportStatusLogger statusLogger,
            int failedRecords, int successRecords, int canceledRecords = 0)
        {
            InitializeRecordCount(statusMessage);

            if (failedRecords > 0) statusMessage.FailRecordsCount += failedRecords;
            if (successRecords > 0) statusMessage.SuccessRecordsCount += successRecords;
            if (canceledRecords > 0) statusMessage.CancelRecordCount += canceledRecords;
            _log.Debug("Updated the status record count at report processor.");
            await statusLogger.LogMessageAsync(statusMessage);
        }

        private static void InitializeRecordCount(ImportStatusMessage statusMessage)
        {
            if (null == statusMessage.FailRecordsCount)
            {
                statusMessage.FailRecordsCount = 0;
            }
            if (null == statusMessage.SuccessRecordsCount)
            {
                statusMessage.SuccessRecordsCount = 0;
            }
            if (null == statusMessage.CancelRecordCount)
            {
                statusMessage.CancelRecordCount = 0;
            }
        }

        private static void AddStatusDetailsWithErrorReponse(MappedFileImportStatusMessage statusMessage, ErrorResultData errorResultData)
        {
            statusMessage.IssueId = errorResultData?.ErrorResponse?.CorrelationId.ToString();
            statusMessage.IssueStatus = errorResultData?.ErrorResponse?.Status;
            statusMessage.IssueTitle = errorResultData?.ErrorResponse?.Title;
            statusMessage.IssueDetail = errorResultData?.ErrorResponse?.Detail;

            if (errorResultData?.ErrorResponse?.Source == null || !errorResultData.ErrorResponse.Source.Any())
            {
                AddStatusDetails(statusMessage, errorResultData.RowNumber, "N/A", errorResultData?.ErrorResponse?.Detail, errorResultData.ImportType);
                return;
            }

            foreach (var pair in errorResultData.ErrorResponse.Source)
            {
                AddStatusDetails(statusMessage, errorResultData.RowNumber, pair.Key, pair.Value, errorResultData.ImportType);
            }
        }

        private static void AddStatusDetails(MappedFileImportStatusMessage statusMessage, int rowNumber, string columnName, string issue, string importType)
        {
            var issueSourceDetails = new MappedFileImportErrorSourceDetail
            {
                RowNumber = rowNumber,
                ColumnName = columnName,
                Issue = issue,
                ImportType = importType
            };
            statusMessage.StatusDetails.Add(issueSourceDetails);
        }

        private static void AddStatusDetailsWithApiResponse(MappedFileImportStatusMessage statusMessage, ErrorResultData errorResultData)
        {
            if (errorResultData?.FailedRecord == null)
            {
                return;
            }
            var failedRecord = errorResultData.FailedRecord;

            const string rowNumberKey = ImportConstants.XlsxFailedRecordOriginalRowColumnName;
            const string otherErrorsKey = ImportConstants.XlsxFailedRecordOtherErrors;

            int rowNumber;
            var success = int.TryParse(failedRecord.CustomData[rowNumberKey], out rowNumber);
            if (!success)
            {
                rowNumber = 0;
            }

            if (failedRecord.Errors != null)
            {
                foreach (var error in failedRecord.Errors)
                {
                    AddStatusDetails(statusMessage, rowNumber, error.Key, error.Value, errorResultData.ImportType);
                }
            }

            if (failedRecord.CustomData != null && !failedRecord.CustomData.ContainsKey(otherErrorsKey)) return;

            var otherErrors = failedRecord.CustomData[otherErrorsKey];
            AddStatusDetails(statusMessage, rowNumber, "N/A", otherErrors, errorResultData.ImportType);

        }

        private void AddErrorStatusDetails(MappedFileImportStatusMessage statusMessage, ErrorResultData errorResultData)
        {
            if (errorResultData == null)
            {
                return;
            }
            if (errorResultData.HttpExporterResult != null)
            {
                AddStatusDetailsWithApiResponse(statusMessage, errorResultData);
            }
            else
            {
                AddStatusDetailsWithErrorReponse(statusMessage, errorResultData);
            }
        }

        private byte[] GenerateFailedData(List<FailedRecord> failedRecords)
        {
            try
            {
                if (failedRecords.Any() && _failedRecordFormatter != null)
                {
                    return _failedRecordFormatter.GenerateXlsxData(failedRecords);
                }
                return null;

            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(Reporter)}:{nameof(GenerateFailedData)}", ex);
                return null;
            }
        }

        private byte[] GenerateFailedDataForMultiSheet(IDictionary<string, IList<FailedRecord>> failedRecorDictionary)
        {
            try
            {
                if (failedRecorDictionary.Values.Any() && _failedRecordFormatter != null)
                {
                    return _failedRecordFormatter.GenerateXlsxData(failedRecorDictionary);
                }
                return null;

            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(Reporter)}:{nameof(GenerateFailedData)}", ex);
                return null;
            }
        }


    }
}
