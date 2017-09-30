using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Paycor.Storage.Blob;

namespace Paycor.Import.Azure
{
    public static class BlobHelper
    {
        public static void SendTextToBlob(
            string blobName,
            string data,
            string connectionString,
            string container)
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(data)))
            {
                StreamToBlob(blobName, stream, connectionString, container);
            }
        }


        public static async Task SendEncryptedTextToBlobAsync(Guid blobName, string data, string containerName,
            string fileName, int clientId, string secretName, string tagName = null)
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(data)))
            {
                if (CheckIfBlobContainerExists(containerName, ConfigurationManager.AppSettings["BlobStorageConnection"]))
                {
                    var blobOperationContext = BlobOperationContext(containerName);
                    var metadata = new BlobMetadata
                    {
                        BlobName = blobName,
                        ClientId = clientId,
                        FileName = fileName,
                        Tag = tagName
                    };
                    await blobOperationContext.UploadBlobAsync(stream, metadata, secretName);
                }
                else
                {
                    throw new ImportException();
                }
            }
        }

        public static void SendEncryptedTextToBlob(Guid blobName, string data, string containerName,
           string fileName, int clientId, string secretName, string tagName = null)
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(data)))
            {
                if (CheckIfBlobContainerExists(containerName, ConfigurationManager.AppSettings["BlobStorageConnection"]))
                {
                    var blobOperationContext = BlobOperationContext(containerName);
                    var metadata = new BlobMetadata
                    {
                        BlobName = blobName,
                        ClientId = clientId,
                        FileName = fileName,
                        Tag = tagName
                    };
                    blobOperationContext.UploadBlobAsync(stream, metadata, secretName).Wait();
                }
                else
                {
                    throw new ImportException($"Attempt to upload file to container:{containerName}, which doesn't exists!");
                }
            }
        }

        public static string GetEncryptedTextFromBlob(Guid blobName, string secretName, string containerName)
        {
            using (var memoryStream = new MemoryStream())
            {
                var blobOperationContext = BlobOperationContext(containerName);
                blobOperationContext.DownloadBlobAsync(memoryStream, blobName, secretName).Wait();

                memoryStream.Flush();
                memoryStream.Position = 0;
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public static async Task<string> GetEncryptedTextFromBlobAsync(Guid blobName, string secretName, string containerName)
        {
            using (var memoryStream = new MemoryStream())
            {
                var blobOperationContext = BlobOperationContext(containerName);
                await blobOperationContext.DownloadBlobAsync(memoryStream, blobName, secretName).ConfigureAwait(false);

                memoryStream.Flush();
                memoryStream.Position = 0;
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        private static BlobOperationContext BlobOperationContext(string containerName)
        {
            var blobStorageConnection = ConfigurationManager.AppSettings["BlobStorageConnection"];
            var vaultUrl = ConfigurationManager.AppSettings["Paycor.Storage.Blob.KeyVaultUrl"];
            var keyVaultUserId = ConfigurationManager.AppSettings["Paycor.Storage.Blob.KeyVaultUserId"];
            var keyVaultUserPassword = ConfigurationManager.AppSettings["Paycor.Storage.Blob.KeyVaultUserPassword"];

            Ensure.ThatStringIsNotNullOrEmpty(blobStorageConnection, nameof(blobStorageConnection));
            Ensure.ThatStringIsNotNullOrEmpty(vaultUrl, nameof(vaultUrl));
            Ensure.ThatStringIsNotNullOrEmpty(keyVaultUserId, nameof(keyVaultUserId));
            Ensure.ThatStringIsNotNullOrEmpty(keyVaultUserPassword, nameof(keyVaultUserPassword));

            var blobOperationContext = new BlobOperationContext(containerName, vaultUrl, keyVaultUserId,
                keyVaultUserPassword, blobStorageConnection);
            return blobOperationContext;
        }

        private static bool CheckIfBlobContainerExists(string containerName, string blobStorageConnection)
        {
            Ensure.ThatStringIsNotNullOrEmpty(containerName, nameof(containerName));
            Ensure.ThatStringIsNotNullOrEmpty(blobStorageConnection, nameof(blobStorageConnection));

            var storageAccount = CloudStorageAccount.Parse(blobStorageConnection);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = blobClient.GetContainerReference(containerName);
            cloudBlobContainer.CreateIfNotExists();
            return cloudBlobContainer.Exists();
        }


        public static void StreamToBlob(string blobName,
            Stream stream,
            string connectionString,
            string container)
        {
            CloudBlobContainer cloudBlobContainer;

            if ((connectionString.ToLower().StartsWith("defaultendpointsprotocol")) ||
                (String.Equals(connectionString, "UseDevelopmentStorage=true", StringComparison.CurrentCultureIgnoreCase)))
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                cloudBlobContainer = blobClient.GetContainerReference(container);
            }
            else
            {
                cloudBlobContainer = new CloudBlobContainer(new Uri(connectionString));
            }

            try
            {
                cloudBlobContainer.CreateIfNotExists();
            }
            catch (Exception e)
            {
                throw new ImportException(
                    $"Could not create blob storage cloudBlobContainer (likely because using SAS key); please create cloudBlobContainer '{container}' in Azure console. Full exception: {e.Message}");
            }

            var blockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
            blockBlob.UploadFromStream(stream);
        }

        public static string GetTextFromBlob(string blobName, string connectionString,
            string containerName)
        {
            var cloudBlockblob = GetCloudBlockBlob(blobName, connectionString, containerName);

            if (!cloudBlockblob.Exists())
                return string.Empty;

            var blobContent = cloudBlockblob.DownloadTextAsync().Result;
            return blobContent;
        }

        public static async Task<string> GetTextFromBlobAsync(string blobName, string connectionString,
            string containerName)
        {
            var cloudBlockblob = GetCloudBlockBlob(blobName, connectionString, containerName);

            if (!cloudBlockblob.Exists())
                return string.Empty;

            var blobContent = await cloudBlockblob.DownloadTextAsync();
            return blobContent;
        }

        public static byte[] GetStreamFromBlob(string blobName, string connectionString, string containerName)
        {
            using (var stream = new MemoryStream())
            {
                var cloudBlockblob = GetCloudBlockBlob(blobName, connectionString, containerName);

                if (!cloudBlockblob.Exists())
                    return null;

                cloudBlockblob.DownloadToStream(stream);
                return stream.ToArray();
            }
        }

        public static async Task<byte[]> GetEncryptedStreamFromBlobAsync(Guid blobName, string secretName, string containerName)
        {
            using (var memoryStream = new MemoryStream())
            {
                var blobOperationContext = BlobOperationContext(containerName);
                await blobOperationContext.DownloadBlobAsync(memoryStream, blobName, secretName).ConfigureAwait(false);

                memoryStream.Flush();
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }


        public static async Task SendEncryptedStreamToBlobAsync(Guid blobName, Stream stream, string containerName,
            string fileName, int clientId, string secretName, string tagName = null)
        {
            if (CheckIfBlobContainerExists(containerName, ConfigurationManager.AppSettings["BlobStorageConnection"]))
            {
                var blobOperationContext = BlobOperationContext(containerName);
                var metadata = new BlobMetadata
                {
                    BlobName = blobName,
                    ClientId = clientId,
                    FileName = fileName,
                    Tag = tagName
                };
                await blobOperationContext.UploadBlobAsync(stream, metadata, secretName);
            }
            else
            {
                throw new ImportException($"Attempt to upload file to container:{containerName}, which doesn't exists!");
            }

        }

        private static CloudBlockBlob GetCloudBlockBlob(string blobName, string connectionString, string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);

            var cloudBlockblob = container.GetBlockBlobReference(blobName);
            return cloudBlockblob;
        }

        public static async Task DeleteBlobAsync(IEnumerable<string> blobNames, string connectionString,
           string containerName)
        {
            foreach (var blob in blobNames)
            {
                var cloudBlockblob = GetCloudBlockBlob(blob, connectionString, containerName);
                await cloudBlockblob.DeleteIfExistsAsync();
            }
        }
    }
}