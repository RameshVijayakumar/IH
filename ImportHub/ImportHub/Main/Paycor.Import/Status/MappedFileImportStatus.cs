using Paycor.Import.ImportHistory;
//TODO: No unit tests

namespace Paycor.Import.Status
{
    public static class MappedFileImportStatus
    {
        private static MappedFileImportStatusLogger _statusLogger;

        public static MappedFileImportStatusLogger GetStatusEngine(
            string id,
            string container,
            IStatusStorageProvider storageProvider, IImportHistoryService importHistoryService)
        {
            var receiver = new ImportStatusReceiver<MappedFileImportStatusMessage>(container);
            var manager = new StatusManager<MappedFileImportStatusMessage>(receiver, storageProvider);
            var retriever = new ImportStatusRetriever<MappedFileImportStatusMessage>(storageProvider, container);

            _statusLogger = new MappedFileImportStatusLogger(id, receiver, storageProvider, manager, retriever, importHistoryService);
            return _statusLogger;
        }
    }
}