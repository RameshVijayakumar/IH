using Microsoft.Azure.WebJobs;
using Paycor.Import.Messaging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Paycor.Import.Azure;

namespace EmployeeImportWorker
{
    [ExcludeFromCodeCoverage]
    public class Functions : WebJobFunctionsBase<EeImportFileUploadMessage>
    {
        private static IWebJobProcessorFactory _factory; 

        public Functions(JobHostConfiguration configuration,
                         IWebJobProcessorFactory factory)
            : base(configuration, factory.Create() )
        {
            _factory = factory;
        }

        public static async Task ProcessPublicApiServiceBusMessageAsync([ServiceBusTrigger(QueueNames.EmployeeImport)] EeImportFileUploadMessage message)
        {            
            await _factory.Create().ProcessAsync(message);
        }
    }
}