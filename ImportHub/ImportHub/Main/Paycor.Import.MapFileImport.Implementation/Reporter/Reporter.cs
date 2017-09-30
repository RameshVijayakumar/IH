using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.ImportHistory;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Status;

namespace Paycor.Import.MapFileImport.Implementation.Reporter
{
    public class Reporter : IReporter
    {
        private readonly ILog _log;
        private readonly IStatusStorageProvider _statusStorageProvider;
        private readonly IImportHistoryService _importHistoryService;
        private readonly ICalculate _statusCalculator;
        private readonly IReportProcessor _reportProcessor;
        private readonly INotificationMessageClient _notificationMessageClient;
        private readonly string _importDetailUri;
        private MappedFileImportStatusLogger _statusLogger;
        private MappedFileImportStatusMessage _statusMessage;
        private ImportContext _context;
        private List<FailedRecord> _failedRows;
        private List<ErrorResultData> _errorResultDataItems;
        private int _failedRecordCount;
        private int _canceledRecordCount;
        private int _totalChunks;
        private int _totalRecordsCount;
        private int _failedRecordCountSoFar;
        private IEnumerable<string> _apiLinks;
        private double _previousPercent;
        private IDictionary<string, IList<FailedRecord>> _failedRecordDictionay;
        private readonly IProvideClientData<MapFileImportResponse> _provideClientData;
        private List<string> _clientIds;
        private int _successCount;
        private int _payloadCount;
        private int _recordFailedPerChunk;


        public Reporter(ILog log,
            IStatusStorageProvider statusStorageProvider,
            IImportHistoryService importHistoryService,
            ICalculate statusCalculator,
            IReportProcessor reportProcessor,
            IProvideClientData<MapFileImportResponse> provideCLientData,
            INotificationMessageClient notificationMessageClient)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(statusStorageProvider, nameof(statusStorageProvider));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            Ensure.ThatArgumentIsNotNull(statusCalculator, nameof(statusCalculator));
            Ensure.ThatArgumentIsNotNull(reportProcessor, nameof(reportProcessor));
            Ensure.ThatArgumentIsNotNull(provideCLientData, nameof(provideCLientData));
            Ensure.ThatArgumentIsNotNull(notificationMessageClient, nameof(notificationMessageClient));
            _statusStorageProvider = statusStorageProvider;
            _importHistoryService = importHistoryService;
            _statusCalculator = statusCalculator;
            _reportProcessor = reportProcessor;
            _log = log;
            _provideClientData = provideCLientData;
            _clientIds = new List<string>();
            _notificationMessageClient = notificationMessageClient;
            _importDetailUri = ConfigurationManager.AppSettings["ImportDetailUri"];
        }

        public async Task ReportAsync(StepNameEnum step, MapFileImportResponse response)
        {
            var newClientIds = _provideClientData.GetAllClientIds(response);
            newClientIds = newClientIds.Except(_clientIds);

            _clientIds = _clientIds.Concat(newClientIds).ToList();
            if (response.ErrorResultDataItems != null && response.Status != Status.Failure)
            {
                _failedRecordCount += response.ErrorResultDataItems.Count();
                _errorResultDataItems.AddRange(response.ErrorResultDataItems);
                _failedRows.AddRange(response.ErrorResultDataItems.Select(
                    errorResultDataItem => errorResultDataItem.FailedRecord));
                SetFailedRowsDictionaryForMultiSheet(_context, response);
            }

            if (response.Status == Status.Success)
            {
                _log.Info($"{step} completed for import transaction {_context.TransactionId} without errors.");
                SetPropertiesForSuccess(step, response);
            }
            else
            {
                _log.Error($"{step} failed for import transaction {_context.TransactionId}.", response.Error);
                _failedRows.AddRange(_errorResultDataItems.Select(
                 errorResultDataItem => errorResultDataItem.FailedRecord));
                SetPropertiesForFatalError(step, response);
            }
            _previousPercent = await LogProgessStatusAsync(_statusLogger, step, _totalChunks, _previousPercent, _apiLinks);
            SaveFailedData();
        }

        public void Initialize(ImportContext context)
        {
            Ensure.ThatArgumentIsNotNull(context, nameof(context));
            _context = context;
            _statusLogger = MappedFileImportStatus.GetStatusEngine(context.TransactionId,
                ContainerNames.ImportStatus, _statusStorageProvider, _importHistoryService);
            _statusMessage = _statusLogger.RetrieveMessage();
            _statusMessage.Status = MappedFileImportStatusEnum.ImportFileData;
            _statusMessage.PercentComplete = 0;
            _statusMessage.Current = 0;
            _statusMessage.Total = 0;
            _failedRecordCount = 0;
            _canceledRecordCount = 0;
            _successCount = 0;
            _payloadCount = 0;
            _recordFailedPerChunk = 0;
            _totalChunks = 0;
            _totalRecordsCount = 0;
            _previousPercent = 0;
            _failedRecordCountSoFar = 0;
            _apiLinks = null;
            _statusMessage.ApiMappings = context.ApiMapping?.ToList();
            _failedRows = new List<FailedRecord>();
            _errorResultDataItems = new List<ErrorResultData>();
            _failedRecordDictionay = new Dictionary<string, IList<FailedRecord>>();
        }

        private void SetPropertiesForFatalError(StepNameEnum step, MapFileImportResponse response)
        {
            if (step == StepNameEnum.Chunker)
            {
                _failedRecordCount = response.TotalRecordsCount;
                _totalChunks = 1;
            }
            else
            {
                _failedRecordCount = _totalRecordsCount - _failedRecordCount;

            }
            if (response.ErrorResultDataItems != null)
            {
                _errorResultDataItems.AddRange(response.ErrorResultDataItems);
            }
        }

        private void SetPropertiesForSuccess(StepNameEnum step, MapFileImportResponse response)
        {
            if (step == StepNameEnum.Chunker)
            {
                _totalChunks = response.TotalChunks;
                _totalRecordsCount = response.TotalRecordsCount;
                _log.Debug($"Total chunks is {_totalChunks} and total number of records is {_totalRecordsCount}");
            }
            if (step == StepNameEnum.Builder)
            {
                var buildResponse = (BuildDataSourceResponse) response;
                _payloadCount = buildResponse.PayloadCount;
                if(_payloadCount == 0 && _context.IsMultiSheetImport)
                    _errorResultDataItems.Add(SaveErrorResultDataForEmptyFile(buildResponse.ImportType));
            }
            if (step == StepNameEnum.Sender)
            {
                //whatever failed in current chunk is total failed - records failed so far in previous chunks.
                //Success is whatever succeeded so far + records success in current chunk.
                if (_payloadCount != 0)
                {
                    _recordFailedPerChunk = _failedRecordCount - _failedRecordCountSoFar;
                    _successCount += _payloadCount - _recordFailedPerChunk;
                    _failedRecordCountSoFar = _failedRecordCount;
                }
                _apiLinks = ((PayloadSenderResponse)response).ApiLinks;
            }
        }

        public async Task ReportCompletionAsync()
        {
            _log.Debug("Report completion entered.");
            if (_totalChunks == 0)
            {
                _errorResultDataItems.Add(SaveErrorResultDataForEmptyFile());
            }

            await _reportProcessor.UpdateStatusRecordCountAsync(_statusMessage, _statusLogger, _failedRecordCount,
                              _successCount, _canceledRecordCount);
            await SetApiErrorResultToStatusMessageAsync(_statusLogger, _errorResultDataItems);
            SaveFailedData();
            await LogCompletedStatusAsync(_statusLogger, _totalRecordsCount);
            _reportProcessor.SendEvent(_statusMessage);
        }

        private void SaveFailedData()
        {
            if (_context.IsMultiSheetImport)
            {
                _reportProcessor.SaveFailedData(_context.TransactionId, _failedRecordDictionay);
            }
            else
            {
                _reportProcessor.SaveFailedData(_context.TransactionId, _failedRows);
            }
        }

        public void CanceledReport()
        {
            _log.Debug("Report Cancel entered.");
            _canceledRecordCount = _totalRecordsCount - _successCount - _failedRecordCount;
        }

        private async Task SetApiErrorResultToStatusMessageAsync(MappedFileImportStatusLogger statusLogger, IEnumerable<ErrorResultData> errorResultDataItems)
        {
            
                await _reportProcessor.SaveStatusFromApiAsync(_statusMessage, statusLogger, errorResultDataItems);
                _log.Debug("Completed SetApiErrorResultToStatusMessage at Reporter");
        }

        private async Task<double> LogProgessStatusAsync(MappedFileImportStatusLogger statusLogger, StepNameEnum stepName, int totalChunks, double previousPercent = 0, IEnumerable<string> apiLinks = null)
        {
            var percent = _statusCalculator.GetChunkStepPercent(stepName, totalChunks, previousPercent);
            _statusMessage.PercentComplete = Math.Round(Convert.ToDecimal(percent));
            _statusMessage.ApiLink = apiLinks?.FirstOrDefault(x => !string.IsNullOrEmpty(x));
            _statusMessage.ClientId = string.Join(";", _clientIds
                                                .Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList());
            if (!string.IsNullOrEmpty(_statusMessage.ApiLink))
            {
                _log.Debug($"Status ApiLink set to: {_statusMessage.ApiLink}.");
            }
            _log.Debug($"Percent complete is {_statusMessage.PercentComplete} %");

            await statusLogger.LogMessageAsync(_statusMessage);
            return percent;
        }

        private async Task LogCompletedStatusAsync(MappedFileImportStatusLogger statusLogger, int totalRecordsCount)
        {
            _statusMessage.Total = totalRecordsCount;
            _statusMessage.Status = MappedFileImportStatusEnum.ImportComplete;
            await statusLogger.LogMessageAsync(_statusMessage);
            var longMessage = ImportConstants.ProcessCompleteMsgLong.Replace("{{here}}", $"<a href='{_importDetailUri}/{_context.TransactionId}'>here</a>");

            _notificationMessageClient.Send(new Guid(_context.MasterSessionId), ApiKeyData.ApiKey, ApiKeyData.ApiSecretKey,
               ImportConstants.ProcessCompleteMsgPurpose,
               ImportConstants.ProcessCompleteMsgShort,
               ImportConstants.ProcessCompleteMsgMedium,
               longMessage,
               NotificationTypes.ImportCompletion);
            _log.Debug("Completed log completed status at reporter");
        }

        private ErrorResultData SaveErrorResultDataForEmptyFile(string importType = null)
        {
            _log.Debug("Unable to import due to empty data file.");
            var errorResultData = new ErrorResultData
            {
                ErrorResponse = new ErrorResponse { Detail = "Unable to import - empty data file." },
                FailedRecord = null,
                HttpExporterResult = null,
                RowNumber = 0,
                ImportType = importType
            };
            return errorResultData;
        }

        private void SetFailedRowsDictionaryForMultiSheet(ImportContext context, MapFileImportResponse response)
        {
            if (!context.IsMultiSheetImport) return;
            var errorResultData = response?.ErrorResultDataItems?.FirstOrDefault();
            if (errorResultData != null)
            {
                _failedRecordDictionay.ConcatenateData(errorResultData.ImportType, _failedRows);
            }
            _failedRows.Clear();
        }
    }
}
