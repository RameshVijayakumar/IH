using Ninject;
using System.Diagnostics.CodeAnalysis;
using log4net;
using Paycor.Import.Azure;

namespace EmployeeImportWorker
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    [ExcludeFromCodeCoverage]
    class Program
    {
        static IKernel _kernel;
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {

            _kernel = KernelFactory.CreateKernel();

            var log = _kernel.Get<ILog>();
            var keyVaultReader = new KeyVaultReader(log);

            ApiKeyData.ApiKey = keyVaultReader.RetrieveAsync(KeyVaultSecrets.ApiKey).Result;
            ApiKeyData.ApiSecretKey = keyVaultReader.RetrieveAsync(KeyVaultSecrets.ApiSecret).Result;

            var functions = _kernel.Get<Functions>();
            functions.RunAndBlock();
        }
    }
}
