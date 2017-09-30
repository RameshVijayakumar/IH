using System.Threading.Tasks;

namespace Paycor.Import.Azure
{
    public interface IKeyVaultReader
    {
        Task<string> RetrieveAsync(string keyName);
    }
}