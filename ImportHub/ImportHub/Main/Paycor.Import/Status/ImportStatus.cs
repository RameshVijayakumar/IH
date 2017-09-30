//TODO: No unit tests
namespace Paycor.Import.Status
{
    public sealed class ImportStatus<T> where T : class
    {
        private ImportStatusLogger<T> _statusLogger;

        public ImportStatusLogger<T> GetStatusEngine(string id, string container, IStatusStorageProvider storageProvider)
        {
            var receiver = new ImportStatusReceiver<T>(container);
            var manager = new StatusManager<T>(receiver, storageProvider);
            var retriever = new ImportStatusRetriever<T>(storageProvider, container);

            _statusLogger = new ImportStatusLogger<T>(id, receiver, storageProvider, manager, retriever);
            return _statusLogger;
        }
    }
}