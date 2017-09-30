using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Paycor.Import.Azure;
using Paycor.Import.Employee.Status;
using Paycor.Import.FileType;
using Paycor.Import.ImportHistory;
using Paycor.Import.Status;
using Paycor.Import.Status.Azure;

namespace Paycor.Import.Employee.ImportHistory
{
    public class LegacyImportCleanUp : ILegacyCleanUp
    {
        private readonly IImportHistoryService _importHistoryService;
        private readonly IStatusStorageProvider _statusStorageProvider;

        public LegacyImportCleanUp(IImportHistoryService importHistoryService)
        {
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            _importHistoryService = importHistoryService;
            _statusStorageProvider = new BlobStatusStorageProvider(ConfigurationManager.AppSettings["BlobStorageConnection"],
                ContainerNames.ImportStatus);
        }

        public void UpdateStuckLegacyHistoryMessages(IEnumerable<ImportHistoryMessage> historyMessages)
        {
            var processingHistoryMessageIds = historyMessages.Where(t => t.FileType != null && t.Status != null 
                                                                       && t.Status == ImportHistoryStatusEnum.Processing.ToString() 
                                                                       && t.FileType == FileTypeEnum.EmployeeImport.ToString())
                .Select(t => t.TransactionId).ToList();

            var stuckHistoryIds = (from historyMessageId in processingHistoryMessageIds where historyMessageId != null
                                   select _statusStorageProvider.RetrieveStatus(ContainerNames.ImportStatus, historyMessageId) 
                                   into status where status?.Status != null
                                   select JsonConvert.DeserializeObject<EmployeeImportStatusMessage>(status.Status) 
                                   into statusMessage where statusMessage?.Status == EmployeeImportStatusEnum.ImportComplete
                                   select statusMessage.Id).ToList();

            foreach (var stuckHistoryId in stuckHistoryIds)
            {
                var historyMessage = _importHistoryService.GetImportHistory(stuckHistoryId);
                if (historyMessage == null) continue;
                historyMessage.ImportHistoryStatus = ImportHistoryStatusEnum.Completed;

                _importHistoryService.SaveImportHistory(historyMessage);
            }
        }
    }
}