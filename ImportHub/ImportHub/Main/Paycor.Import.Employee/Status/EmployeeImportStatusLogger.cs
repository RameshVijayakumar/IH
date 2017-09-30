using System;
using System.Threading.Tasks;
using Paycor.Import.Extensions;
using Paycor.Import.ImportHistory;
using Paycor.Import.Status;

namespace Paycor.Import.Employee.Status
{
    public class EmployeeImportStatusLogger : ImportStatusLogger<EmployeeImportStatusMessage>
    {
        private readonly ImportStatusReceiver<EmployeeImportStatusMessage> _receiver;
        private readonly ImportStatusRetriever<EmployeeImportStatusMessage> _retriever;
        private readonly IImportHistoryService _importHistoryService;

        private readonly string _id;

        public EmployeeImportStatusLogger(string id,
            ImportStatusReceiver<EmployeeImportStatusMessage> receiver,
            IStatusStorageProvider storageProvider,
            StatusManager<EmployeeImportStatusMessage> manager,
            ImportStatusRetriever<EmployeeImportStatusMessage> retriever,
            IImportHistoryService importHistoryService)
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
            _retriever = retriever;
            _importHistoryService = importHistoryService;
        }

        public void IncrementSuccess()
        {
            var message = _retriever.RetrieveStatus(_id);
            if (message == null)
            {
                throw new ArgumentException(ImportStatusResource.StatusMessageNotFound);
            }
            message.SuccessRecordsCount++;
            LogMessage(message);
        }

        public void IncrementFail()
        {
            var message = _retriever.RetrieveStatus(_id);
            if (message == null)
            {
                throw new ArgumentException(ImportStatusResource.StatusMessageNotFound);
            }
            message.FailRecordsCount++;
            LogMessage(message);
        }

        public void IncrementWarn()
        {
            var message = _retriever.RetrieveStatus(_id);
            if (message == null)
            {
                throw new ArgumentException(ImportStatusResource.StatusMessageNotFound);
            }
            message.WarnRecordCount++;
            LogMessage(message);
        }

        public override void LogMessage(EmployeeImportStatusMessage message)
        {
            _receiver.Send(_id, message);
            LogImportHistory(message);
        }

        public override async Task LogMessageAsync(EmployeeImportStatusMessage message)
        {
            _receiver.Send(_id, message);
            await LogImportHistoryAsync(message);
        }

        public void LogDetail(EmployeeImportStatusDetail detail)
        {
            var message = _retriever.RetrieveStatus(_id);
            if (message == null)
            {
                throw new ArgumentException(ImportStatusResource.StatusMessageNotFound);
            }
            message.StatusDetails.Add(detail);
            LogMessage(message);
        }
    
        private void LogImportHistory(EmployeeImportStatusMessage message)
        {
            var historyMessage = SetImportHistoryMessage(message);
            _importHistoryService.SaveImportHistory(historyMessage);
        }

        private async Task LogImportHistoryAsync(EmployeeImportStatusMessage message)
        {
            var historyMessage = SetImportHistoryMessage(message);
            await _importHistoryService.SaveImportHistoryAsync(historyMessage);
        }

        private ImportHistoryMessage SetImportHistoryMessage(EmployeeImportStatusMessage message)
        {
            int clientId;
            int.TryParse(message.ClientId, out clientId);
            ImportHistoryStatusEnum status;

            if ((message.Status >= (int) EmployeeImportStatusEnum.InitiateEmployeeImport &&
                 message.Status < EmployeeImportStatusEnum.ImportComplete))
            {
                status = ImportHistoryStatusEnum.Processing;
            }
            else
            {
                switch (message.Status)
                {
                    case EmployeeImportStatusEnum.Queued:
                        status = ImportHistoryStatusEnum.Queued;
                        break;
                    case EmployeeImportStatusEnum.ImportComplete:
                        status = ImportHistoryStatusEnum.Completed;
                        break;
                    default:
                        status = ImportHistoryStatusEnum.Unknown;
                        break;
                }
            }

            var historyMessage = _importHistoryService.GetImportHistory(message.Id) ?? new ImportHistoryMessage
                                 {
                                     ClientId = message.ClientId,
                                     User = message.User,
                                     UserName = message.UserName,
                                     FileName = message.FileName,
                                     TransactionId = message.Id,
                                     ImportDate = DateTime.Now,
                                     ImportDateEpoch = DateTime.Now.AsEpoch(),
                                     ImportType = "Employee Import",
                                     FileType = message.FileType,
                                     IsMarkedForDelete = ImportConstants.False
                                 };

            // Always update the following fields.
            historyMessage.ImportHistoryStatus = status;
            historyMessage.ImportedRecordCount = message.SuccessRecordsCount;
            historyMessage.FailedRecordCount = message.FailRecordsCount;
            historyMessage.SummaryErrorMessage = message.SummaryErrorMessage;
            historyMessage.Source = message.Source;
            if (status != ImportHistoryStatusEnum.Completed) return historyMessage;
            historyMessage.ImportCompletionDate = DateTime.Now;
            historyMessage.ImportCompletionDateEpoch = DateTime.Now.AsEpoch();
            return historyMessage;
        }
    }


}



        

