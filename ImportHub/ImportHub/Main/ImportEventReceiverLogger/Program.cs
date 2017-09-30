using System.Configuration;
using log4net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Ninject;

namespace ImportEventReceiverLogger
{
    internal static class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        private static void Main()
        {
            var configuration = new JobHostConfiguration
            {
                DashboardConnectionString = ConfigurationManager.AppSettings["StorageConnection"],
                StorageConnectionString = ConfigurationManager.AppSettings["StorageConnection"]
            };
            configuration.UseServiceBus(new ServiceBusConfiguration
            {
                ConnectionString = ConfigurationManager.AppSettings["PaycorServiceBusConnection"],
            });

            var kernel = KernelFactory.GetKernel();
            var log = kernel.Get<ILog>();

            log.Info("Import Event Receiver Logging process started.");

            var host = new JobHost(configuration);
            host.RunAndBlock();
        }
    }
}
