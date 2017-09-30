using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Paycor.Import.Azure
{
    public class BlobStorageProvider : IStorageProvider
    {
        private string _blobConnectionString;

        private string BlobConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_blobConnectionString))
                {
                    var blobConnectionString = ConfigurationManager.AppSettings["BlobStorageConnection"];
                    if (string.IsNullOrEmpty(blobConnectionString))
                    {
                        throw new Exception("BlobStorageConnection is not configured");
                    }
                    _blobConnectionString = blobConnectionString;
                }

                return _blobConnectionString;
            }
        }

        public void Save(string blobName, string data)
        {
            BlobHelper.SendTextToBlob(blobName, data, BlobConnectionString,
                    ContainerNames.FailedRecords);
        }

        public void SaveStream(string blobName, Stream data)
        {
            BlobHelper.StreamToBlob(blobName, data, BlobConnectionString,
                    ContainerNames.FailedRecords);
        }

        public async Task SendEncryptedTextToBlobAsync(Guid blobName, string data, string containerName, string fileName, int clientId,
            string secretName, string tagName = null)
        {
            await BlobHelper.SendEncryptedTextToBlobAsync(blobName, data, containerName, fileName, clientId, secretName,
                tagName);
        }

        public void SendEncryptedTextToBlob(Guid blobName, string data, string containerName, string fileName, int clientId,
            string secretName, string tagName = null)
        {
            BlobHelper.SendEncryptedTextToBlob(blobName, data, containerName, fileName, clientId, secretName,tagName);
        }

        public void SendEncryptedStreamToBlob(Guid blobName, Stream stream, string containerName, string fileName, int clientId,
           string secretName, string tagName = null)
        {
            BlobHelper.SendEncryptedStreamToBlobAsync(blobName, stream, containerName, fileName, clientId, secretName, tagName).Wait();
        }


        public async Task<string> RetrieveAsync(string blobName)
        {
            return await BlobHelper.GetTextFromBlobAsync(blobName, BlobConnectionString, ContainerNames.FailedRecords);
        }

        public string GetAllBlobNames()
        {
            var storageAccount = CloudStorageAccount.Parse(BlobConnectionString);
            var containers = storageAccount.CreateCloudBlobClient().ListContainers();

            return containers.Select(t => t.Name).Aggregate((i, j) => i + "," + j);
        }

        public string GetAllTableNames()
        {
            var storageAccount = CloudStorageAccount.Parse(BlobConnectionString);
            var tables = storageAccount.CreateCloudTableClient().ListTables();

            return tables.Select(t => t.Name).Aggregate((i, j) => i + "," + j);
        }

        public string Retrieve(string blobName)
        {
            return BlobHelper.GetTextFromBlob(blobName, BlobConnectionString, ContainerNames.FailedRecords);
        }

        public byte[] RetrieveStream(string blobName)
        {
            return BlobHelper.GetStreamFromBlob(blobName, BlobConnectionString, ContainerNames.FailedRecords);
        }

        public async Task<string> GetEncryptedTextFromBlobAsync(Guid blobName, string secretName, string containerName
          )
        {
            var data = await BlobHelper.GetEncryptedTextFromBlobAsync(blobName, secretName, containerName);
            return data;
        }

        public async Task<byte[]> GetEncryptedStreamFromBlobAsync(Guid blobName, string secretName, string containerName
          )
        {
            var data = await BlobHelper.GetEncryptedStreamFromBlobAsync(blobName, secretName, containerName);
            
            return data;        
        }

        public string GetEncryptedTextFromBlob(Guid blobName, string secretName, string containerName)
        {
            return BlobHelper.GetEncryptedTextFromBlob(blobName, secretName, containerName);
        }

        public async Task DeleteTextFromBlobAsync(IEnumerable<string> blobNames, string containerName)
        {
            await BlobHelper.DeleteBlobAsync(blobNames, BlobConnectionString, containerName);
        }      
    }
}
