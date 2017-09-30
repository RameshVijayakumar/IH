
using System.Threading.Tasks;

namespace Paycor.Import.MapFileImport.Implementation.Reporter
{
    public interface IReporter
    {
        Task ReportAsync(StepNameEnum step, MapFileImportResponse response);

        void Initialize(ImportContext context);

        Task ReportCompletionAsync();

        void CanceledReport();
    }
}
