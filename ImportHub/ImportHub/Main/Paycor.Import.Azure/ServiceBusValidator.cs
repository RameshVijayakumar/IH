using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Paycor.SystemCheck;

namespace Paycor.Import.Azure
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ServiceBusValidator : IEnvironmentValidator
    {
        public IEnumerable<EnvironmentValidation> EnvironmentValidate()
        {
            var appsettingKey = "ImportHubServiceBusConnection";
            var sbConnectionString = ConfigurationManager.AppSettings[appsettingKey];
            var validations = new List<EnvironmentValidation>();
            var queues = new []
            {
                QueueNames.EmployeeImport,
                QueueNames.MappedFileImport,
                QueueNames.MultiFileImport
            };

            foreach (var queue in queues)
            {
                try
                {
                    var client = QueueClient.CreateFromConnectionString(sbConnectionString, queue);
                    var namespaceMgr = NamespaceManager.CreateFromConnectionString(sbConnectionString);
                    var count = namespaceMgr.GetQueue(queue).MessageCountDetails.ActiveMessageCount;

                    validations.Add(new EnvironmentValidation
                    {
                        CurrentSetting = appsettingKey,
                        Name = $"Service Bus Queue: {queue}",
                        Result = EnvironmentValidationResult.Pass,
                        AdditionalInformation = $"Active message count: {count}"
                    });
                }
                catch (Exception ex)
                {
                    validations.Add(new EnvironmentValidation
                    {
                        CurrentSetting = appsettingKey,
                        Name = $"Service Bus Queue: {queue}",
                        Result = EnvironmentValidationResult.Fail,
                        AdditionalInformation = ex.ToString()
                    });
                }
            }
            return validations;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
