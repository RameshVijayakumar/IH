using System;
using System.IO;
using System.Net.Http;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Paycor.Import.Azure
{
    public class BlobStoreFile : IStoreFile
    {
        private readonly string _containerName;
        private readonly string _connectionString;

        public BlobStoreFile(string containerName, string connectionString)
        {
            Ensure.ThatStringIsNotNullOrEmpty(containerName, nameof(containerName));
            Ensure.ThatStringIsNotNullOrEmpty(connectionString, nameof(connectionString));
            _containerName = containerName;
            _connectionString = connectionString;
        }

        public void StoreFile(HttpPostedFile httpPostedFile)
        {
            Ensure.ThatArgumentIsNotNull(httpPostedFile, nameof(httpPostedFile));
            var blobName = Path.GetFileName(httpPostedFile.FileName);

            BlobHelper.StreamToBlob(blobName,
                httpPostedFile.InputStream,
                _connectionString,
                _containerName);
        }

        public void StoreFile(MultipartFileData multipartFileData)
        {
            Ensure.ThatArgumentIsNotNull(multipartFileData, nameof(multipartFileData));
            var localFile = multipartFileData.LocalFileName;
            var blobName = Path.GetFileName(localFile);
            using (var stream = new FileStream(localFile, FileMode.Open))
            {
                // This change is intentional. A local version of StreamToBlob was created
                // (as opposed to using the exiting StreamToBlob method in BlobHelper class)
                // in order to test using an "Exponential Retry policy" to deal with the problem of
                // initial failure of writing the stream the first time the application is deployed.
                // If this fix is successful in addressing the problem, this new method will be merged
                // back into the base shareable component as a nuget update.
                StreamToBlob(blobName, stream);
            }
        }

        private void StreamToBlob(string blobName, Stream stream)
        {
            var cloudBlobContainer = _connectionString.ToLower()
                .StartsWith("defaultendpointsprotocol") || string.Equals(_connectionString, "UseDevelopmentStorage=true", StringComparison.CurrentCultureIgnoreCase)
                ? CloudStorageAccount.Parse(_connectionString).CreateCloudBlobClient().GetContainerReference(_containerName)
                : new CloudBlobContainer(new Uri(_connectionString));
            try
            {
                cloudBlobContainer.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                throw new ImportException(string.Format("Could not create blob storage cloudBlobContainer (likely because using SAS key); please create cloudBlobContainer '{0}' in Azure console. Full exception: {1}",
                    _containerName, ex.Message));
            }

            var options = new BlobRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 5)
            };

            cloudBlobContainer.GetBlockBlobReference(blobName).UploadFromStream(stream, options: options);
        }
    }
}
