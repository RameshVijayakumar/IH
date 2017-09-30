using System.Diagnostics.CodeAnalysis;
using System.IO;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Paycor.Import.FileDataExtracter.LegacyShim;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    [ExcludeFromCodeCoverage]
    public class BaseImportImporter
    {
        private readonly IFileDataExtracterFactory<ImportContext> _fileDataExtracterFactory;
        private readonly string _storageConnectionString;

        protected ILog Log { get; }

        protected IFileDataExtracterFactory<ImportContext> FileDataExtracterFactory => _fileDataExtracterFactory;

        protected BaseImportImporter(ILog log, IFileDataExtracterFactory<ImportContext> fileDataExtracterFactory, string storageConnectionString)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(fileDataExtracterFactory, nameof(fileDataExtracterFactory));
            Ensure.ThatStringIsNotNullOrEmpty(storageConnectionString, nameof(storageConnectionString));

            log.Debug($"{nameof(MappedFileImportImporter)} constructor entered.");

            _storageConnectionString = storageConnectionString;
            _fileDataExtracterFactory = fileDataExtracterFactory;
            _fileDataExtracterFactory.LoadHandlers(log);
            Log = log;
        }

        protected CloudBlockBlob GetCloudBlockBlob(ImportContext context)
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created cloudBlobContainer.
            var container = blobClient.GetContainerReference(context.Container);

            // Retrieve reference to a blob named "photo1.jpg".
            return (container.GetBlockBlobReference(context.FileName));
        }

        protected void RemoveBlobFromStorage(ICloudBlob blob)
        {
            blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }

        protected MemoryStream GetStreamFromBlob(CloudBlockBlob blockBlob)
        {
            var memoryStream = new MemoryStream();

            if (!blockBlob.Exists())
            {
                throw new FileNotFoundException(
                    $"The uploaded file {blockBlob.Name} was not found at the expected location {blockBlob.Container.Name}.");
            }

            blockBlob.DownloadToStream(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
