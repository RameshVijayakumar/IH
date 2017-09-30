using System.Collections.Generic;
using System.Threading.Tasks;
using Paycor.Import.Azure;


namespace Paycor.Import.Status.Azure
{
    public class BlobStatusStorageProvider : IStatusStorageProvider
    {
        private readonly string _connectionString;
        private readonly string _rootContainerName;

        public BlobStatusStorageProvider(string connectionString, string rootContainerName)
        {
            Ensure.ThatStringIsNotNullOrEmpty(connectionString, nameof(connectionString));
            Ensure.ThatStringIsNotNullOrEmpty(rootContainerName, nameof(rootContainerName));

            _connectionString = connectionString;
            _rootContainerName = rootContainerName;
        }

        public StatusMessage RetrieveStatus(string containerName, string blobName)
        {
            Ensure.ThatStringIsNotNullOrEmpty(containerName, nameof(containerName));
            Ensure.ThatStringIsNotNullOrEmpty(blobName, nameof(blobName));

            var status = BlobHelper.GetTextFromBlob(blobName, _connectionString, containerName);

            return new StatusMessage
            {
                Reporter = containerName,
                Key = blobName,
                Status = status
            };                
        }

        public void StoreStatus(StatusMessage statusMessage)
        {
            Ensure.ThatArgumentIsNotNull(statusMessage, nameof(statusMessage));
            Ensure.ThatPropertyIsInitialized(statusMessage.Reporter, nameof(statusMessage.Reporter));
            Ensure.ThatPropertyIsInitialized(statusMessage.Key, nameof(statusMessage.Key));


            BlobHelper.SendTextToBlob(statusMessage.Key, statusMessage.Status, _connectionString, statusMessage.Reporter);
        }

        public async Task DeleteStatusAsync(string containerName, IEnumerable<string> blobNames)
        {
            Ensure.ThatStringIsNotNullOrEmpty(containerName, nameof(containerName));

            await BlobHelper.DeleteBlobAsync(blobNames, _connectionString, containerName);

        }
    }
}
