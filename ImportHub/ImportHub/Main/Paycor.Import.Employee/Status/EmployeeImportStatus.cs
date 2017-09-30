using Paycor.Import.ImportHistory;
using Paycor.Import.Status;

namespace Paycor.Import.Employee.Status
{
    public static class EmployeeImportStatus
    {
        static EmployeeImportStatusLogger _statusLogger; 

        public static EmployeeImportStatusLogger GetStatusEngine(
            string id,
            string container,
            IStatusStorageProvider storageProvider, IImportHistoryService importHistoryService)
        {
          
            var receiver = new ImportStatusReceiver<EmployeeImportStatusMessage>(container);
            var manager = new StatusManager<EmployeeImportStatusMessage>(receiver, storageProvider);
            var retriever = new ImportStatusRetriever<EmployeeImportStatusMessage>(storageProvider, container);

            _statusLogger = new EmployeeImportStatusLogger(id, receiver, storageProvider, manager, retriever, importHistoryService);
            return _statusLogger;
        }
    }
}