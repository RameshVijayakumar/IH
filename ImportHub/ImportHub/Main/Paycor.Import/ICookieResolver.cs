using System.Linq;
using System.Threading.Tasks;

namespace Paycor.Import
{
    public interface ICookieResolver
    {
        Task<string> ResolveAsync(string apiKey, string apiSecretKey);
    }
}