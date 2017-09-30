using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace Paycor.Import.Azure
{

    public class FileStorageProvider : IFileStorageProvider
    {
        private string _fileConnectionString;

        private string FileConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_fileConnectionString))
                {
                    var fileConnectionString = ConfigurationManager.AppSettings["StorageConnection"];
                    if (string.IsNullOrEmpty(fileConnectionString))
                    {
                        throw new Exception("StorageConnection is not configured");
                    }
                    _fileConnectionString = fileConnectionString;
                }

                return _fileConnectionString;
            }
        }

        public async Task<byte[]> GetFileFromFileStorage(string fileName, string objectType)
        {
            using (var memory = new MemoryStream())
            {
                var file = GetCloudFile(FileConnectionString, fileName, objectType);

                if (file != null)
                {
                    if (file.Exists())
                    {
                        await file.DownloadToStreamAsync(memory).ConfigureAwait(false);
                    }
                }
                

                memory.Flush();
                memory.Position = 0;
                return memory.ToArray();
            }
            
        }

        private static CloudFile GetCloudFile(string fileConnectionString, string fileName, string objectType)
        {
            var fileStorage = CloudStorageAccount.Parse(fileConnectionString);

            var fileClient = fileStorage.CreateCloudFileClient();

            var fileShare = fileClient.GetShareReference(FileShareStorage.ImportHubFileShare);
            fileShare.CreateIfNotExistsAsync().ConfigureAwait(false);

            var templatesDirectory = fileShare.GetRootDirectoryReference().GetDirectoryReference(objectType);
            
            return templatesDirectory.Exists() ? templatesDirectory.GetFileReference(fileName) : null;
        }
    }
}
