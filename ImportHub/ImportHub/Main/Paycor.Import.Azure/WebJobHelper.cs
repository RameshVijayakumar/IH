using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace Paycor.Import.Azure
{
    public static class WebJobHelper
    {
        public static JobHostConfiguration GetJobHostConfiguration()
        {
            var jobHostConfiguration = new JobHostConfiguration
            {
                StorageConnectionString = AzureConfiguration.StorageConnectionString,
                DashboardConnectionString = AzureConfiguration.AzureWebJobsDashboardConnection
            };
            jobHostConfiguration.UseServiceBus(new ServiceBusConfiguration
            {
                ConnectionString = AzureConfiguration.ServiceBusConnectionString,
            });

#if DEBUG
            Console.WriteLine(@"StorageConnectionString: {0}", AzureConfiguration.StorageConnectionString.Split(';')[1].Split('=')[1]);
            Console.WriteLine(@"ServiceBusConnectionString: {0}", AzureConfiguration.ServiceBusConnectionString.Split(';')[0].Split('=')[1]);
            Console.WriteLine(@"DashboardConnectionString: {0}", jobHostConfiguration.DashboardConnectionString.Split(';')[1].Split('=')[1]);
            Console.WriteLine();
#endif
            return (jobHostConfiguration);
        }

    }
}
