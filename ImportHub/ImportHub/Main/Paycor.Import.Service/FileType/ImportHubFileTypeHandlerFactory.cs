using System.Configuration;
using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.FileType;
using Paycor.Import.ImportHistory;
using Paycor.Import.JsonFormat;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using Paycor.Import.Status.Azure;

namespace Paycor.Import.Service.FileType
{
    public sealed class ImportHubFileTypeHandlerFactory : BaseFileTypeHandlerFactory
    {
        private readonly ILog _log;
        private readonly IImportHistoryService _importHistoryService;
        private readonly IFieldMapper _fieldMapper;

        public ImportHubFileTypeHandlerFactory(
            ILog log,
            IImportHistoryService importHistoryService,
            IFieldMapper fieldMapper)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            Ensure.ThatArgumentIsNotNull(fieldMapper, nameof(fieldMapper));

            _log = log;
            _importHistoryService = importHistoryService;
            _fieldMapper = fieldMapper;
            LoadHandlers();
        }

        public override void LoadHandlers()
        {
            var blobConnection = ConfigurationManager.AppSettings["BlobStorageConnection"];
            var sbConnection = ConfigurationManager.AppSettings["ImportHubServiceBusConnection"];

            var eeImportFileStore = new BlobStoreFile(ContainerNames.EmployeeImport, blobConnection);
            var mappedImportFileStore = new BlobStoreFile(ContainerNames.MappedFileImport, blobConnection);
            var eeClient = new ServiceBusQueueClient<FileUploadMessage>();
            var mappedClient = new ServiceBusQueueClient<FileUploadMessage>();
            var jsonFormatter = new JsonFormatter();
            var statusStorage = new BlobStatusStorageProvider(blobConnection, ContainerNames.ImportStatus);
            var cancelStorage = new ImportCancelTokenStorage(blobConnection, TableNames.Cancellations,
                PartitionKeys.CancelTokenKey);

            AddFileTypeHandler(new EmployeeImportFileTypeHandler(_log, eeImportFileStore, eeClient,
                _importHistoryService, sbConnection, jsonFormatter));
            AddFileTypeHandler(new MappedFileTypeHandler(_log, mappedImportFileStore, mappedClient,
                _importHistoryService, _fieldMapper, sbConnection, jsonFormatter, cancelStorage, statusStorage));
            AddFileTypeHandler(new UnrecognizedFileTypeHandler());
        }
    }
}