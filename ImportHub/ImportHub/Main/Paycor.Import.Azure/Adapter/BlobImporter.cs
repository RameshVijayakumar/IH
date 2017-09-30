using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Paycor.Import.Adapter;
using Paycor.Import.Messaging;


namespace Paycor.Import.Azure.Adapter
{
    public abstract class BlobImporter<TOutData, TMessage> : Importer<TOutData> where TMessage : FileUploadMessage
    {
        private const string StorageConnectionStringKey = "BlobStorageConnection";

        private string StorageConnectionString { get; set; }

        protected ILog Log { get; private set; }

        protected BlobImporter(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));

            Log = log;
        }

        public override bool Initialize(NameValueCollection configSettings)
        {
            StorageConnectionString = CloudConfigurationManager.GetSetting(StorageConnectionStringKey);

            if (String.IsNullOrEmpty(StorageConnectionString))
                Log.ErrorFormat("CsvBlobImporter: Blob storage connection string not defined. Verify the {0} key is defined in the configuration and has a valid value.", StorageConnectionStringKey);

            return base.Initialize(configSettings);
        }

        protected override async Task<TOutData> OnImportAsync(dynamic descriptor)
        {
            var blockBlob = GetCloudBlockBlob(descriptor);

            var textReader = GetDataFromBlob(blockBlob);

            var data = await ImportDataAsync(descriptor, textReader);

            RemoveBlobFromStorage(blockBlob);

            return data;
        }

        protected virtual TextReader GetDataFromBlob(CloudBlockBlob blockBlob)
        {
            return (new StringReader(blockBlob.DownloadText()));
        }

        protected CloudBlockBlob GetCloudBlockBlob(TMessage message)
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created cloudBlobContainer.
            var container = blobClient.GetContainerReference(message.Container);

            // Retrieve reference to a blob named "photo1.jpg".
            return (container.GetBlockBlobReference(message.File));
        }

        protected abstract Task<TOutData> ImportDataAsync(TMessage message, TextReader textReader);

        protected void RemoveBlobFromStorage(ICloudBlob blob)
        {
            blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }
    }
}

