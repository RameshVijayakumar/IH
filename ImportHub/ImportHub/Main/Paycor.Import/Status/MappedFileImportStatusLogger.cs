using System;
using System.Threading.Tasks;
using Paycor.Import.Extensions;
using Paycor.Import.ImportHistory;
//TODO: No unit tests

namespace Paycor.Import.Status
{
    public class MappedFileImportStatusLogger : ImportStatusLogger<MappedFileImportStatusMessage>
    {
        private readonly ImportStatusReceiver<MappedFileImportStatusMessage> _receiver;
        private readonly IImportHistoryService _importHistoryService;

        private readonly string _id;

        public MappedFileImportStatusLogger(string id,
            ImportStatusReceiver<MappedFileImportStatusMessage> receiver,
            IStatusStorageProvider storageProvider,
            StatusManager<MappedFileImportStatusMessage> manager,
            ImportStatusRetriever<MappedFileImportStatusMessage> retriever, IImportHistoryService importHistoryService)
            : base(id, receiver, storageProvider, manager, retriever)
        {
            Ensure.ThatStringIsNotNullOrEmpty(id, nameof(id));
            Ensure.ThatArgumentIsNotNull(receiver, nameof(receiver));
            Ensure.ThatArgumentIsNotNull(storageProvider, nameof(storageProvider));
            Ensure.ThatArgumentIsNotNull(manager, nameof(manager));
            Ensure.ThatArgumentIsNotNull(retriever, nameof(retriever));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));

            _id = id;
            _receiver = receiver;
            _importHistoryService = importHistoryService;
        }

        public async Task LogMessageAsync(MappedFileImportStatusMessage message, string failedRecordsFile = null)
        {
            _receiver.Send(_id, message);
            await LogImportHistoryAsync(message, failedRecordsFile);
        }

        public void LogMessage(MappedFileImportStatusMessage message, string failedRecordsFile = null)
        {
            _receiver.Send(_id, message);
            LogImportHistory(message, failedRecordsFile);
        }

        private async Task LogImportHistoryAsync(MappedFileImportStatusMessage message, string failedRecordsFile = null)
        {
            var historyMessage = SetImportHistoryMessage(message);
            await _importHistoryService.SaveImportHistoryAsync(historyMessage, failedRecordsFile);
        }

        private void LogImportHistory(MappedFileImportStatusMessage message, string failedRecordsFile = null)
        {
            var historyMessage = SetImportHistoryMessage(message);
            _importHistoryService.SaveImportHistory(historyMessage, failedRecordsFile);
        }

        private ImportHistoryMessage SetImportHistoryMessage(MappedFileImportStatusMessage message)
        {
            ImportHistoryStatusEnum status;

            switch (message.Status)
            {
                case MappedFileImportStatusEnum.Queued:
                    status = ImportHistoryStatusEnum.Queued;
                    break;
                case MappedFileImportStatusEnum.ImportFileData:
                    status = ImportHistoryStatusEnum.Processing;
                    break;
                case MappedFileImportStatusEnum.PrevalidationFailure:
                case MappedFileImportStatusEnum.ProcessingFailure:
                case MappedFileImportStatusEnum.ImportComplete:
                    status = ImportHistoryStatusEnum.Completed;
                    break;
                default:
                    status = ImportHistoryStatusEnum.Unknown;
                    break;
            }

            var historyMessage = _importHistoryService.GetImportHistory(message.Id) ?? new ImportHistoryMessage
            {
                User = message.User,
                ClientId = message.ClientId,
                UserName = message.UserName,
                FileName = message.FileName,
                TransactionId = message.Id,
                ImportDate = DateTime.Now,
                ImportDateEpoch = DateTime.Now.AsEpoch(),
                ImportType = message.ImportType,
                FileType = message.FileType,
                IsMarkedForDelete = ImportConstants.False
            };

            // Always update the following fields.
            historyMessage.ImportHistoryStatus = status;
            historyMessage.ImportedRecordCount = message.SuccessRecordsCount;
            historyMessage.FailedRecordCount = message.FailRecordsCount;
            historyMessage.CancelledRecordCount = message.CancelRecordCount;
            historyMessage.Source = message.Source;
            historyMessage.ClientId = message.ClientId;

            if (status == ImportHistoryStatusEnum.Completed)
            {
                historyMessage.ImportCompletionDate = DateTime.Now;
                historyMessage.ImportCompletionDateEpoch = DateTime.Now.AsEpoch();
            }
            return historyMessage;
        }
    }
}