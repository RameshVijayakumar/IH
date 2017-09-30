using System.Net.Http;
using System.Web;

namespace Paycor.Import
{
    /// <summary>
    /// Defines the methods that need to be implemented when writing
    /// a class that can be used by the FileUploadController to store
    /// files when they have been received.
    /// </summary>
    public interface IStoreFile
    {
        void StoreFile(HttpPostedFile httpPostedFile);

        void StoreFile(MultipartFileData multipartFileData);
    }
}
