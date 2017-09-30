using System;
using System.Configuration;
using log4net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Ninject;
using Paycor.Import.Azure;

namespace MappedFileImportProcessor
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    internal class Program
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
                ConnectionString = ConfigurationManager.AppSettings["ServiceBusConnection"],
                MessageOptions = new OnMessageOptions
                {
                    MaxConcurrentCalls = int.Parse(ConfigurationManager.AppSettings["MaxConcurrentCalls"]),
                    AutoRenewTimeout = TimeSpan.FromDays(1),
                    AutoComplete = true
                },
            });

            var kernel = KernelFactory.GetKernel();
            var log = kernel.Get<ILog>();

            log.Info("Mapped File Import Processor started.");

            var keyVaultReader = new KeyVaultReader(log);

            ApiKeyData.ApiKey = keyVaultReader.RetrieveAsync(KeyVaultSecrets.ApiKey).Result;
            ApiKeyData.ApiSecretKey = keyVaultReader.RetrieveAsync(KeyVaultSecrets.ApiSecret).Result;

            var host = new JobHost(configuration);
            host.RunAndBlock();
        }
    }
}
