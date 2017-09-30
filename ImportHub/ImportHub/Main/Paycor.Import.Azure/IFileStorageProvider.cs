using System.Threading.Tasks;

namespace Paycor.Import.Azure
{
    public interface IFileStorageProvider
    {
        Task<byte[]> GetFileFromFileStorage(string fileName, string objectType);
    }
}
