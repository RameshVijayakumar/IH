using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Paycor.Import
{
    public interface IStorageProvider
    {
        void Save(string blobName, string data);
        string Retrieve(string blobName);
        Task<string> RetrieveAsync(string blobName);
        string GetAllBlobNames();
        string GetAllTableNames();
        Task<string> GetEncryptedTextFromBlobAsync(Guid blobName, string secretName, string containerName);
        Task SendEncryptedTextToBlobAsync(Guid blobName, string data, string containerName, string fileName, int clientId, string secretName, string tagName = null);
        void SendEncryptedTextToBlob(Guid blobName, string data, string containerName, string fileName, int clientId, string secretName, string tagName = null);
        string GetEncryptedTextFromBlob(Guid blobName, string secretName, string containerName);
        void SaveStream(string blobName, Stream data);
        void SendEncryptedStreamToBlob(Guid blobName, Stream stream, string containerName, string fileName, int clientId,
            string secretName, string tagName = null);

        byte[] RetrieveStream(string blobName);

        Task<byte[]> GetEncryptedStreamFromBlobAsync(Guid blobName, string secretName, string containerName
            );
        Task DeleteTextFromBlobAsync(IEnumerable<string> blobNames, string containerName);
    }
}
