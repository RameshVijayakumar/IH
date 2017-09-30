
using System.Threading.Tasks;

namespace Paycor.Import.Azure
{
    public interface IWebJobProcessor<in TBody>
    {
        void Process(TBody body);

        Task ProcessAsync(TBody body);
    }
}
