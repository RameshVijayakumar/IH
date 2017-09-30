using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Paycor.SystemCheck;

namespace Paycor.Import.Azure
{
    public class MappedFileTopicEnvironmentValidator : IEnvironmentValidator
    {
        public IEnumerable<EnvironmentValidation> EnvironmentValidate()
        {
            var items = new List<EnvironmentValidation>();
            try
            {
                var connectionString = ConfigurationManager.AppSettings["ServiceBusConnection"];
                var client = TopicClient.CreateFromConnectionString(connectionString, MappedFileTopicInfo.TopicName);
                var namespaceMgr = NamespaceManager.CreateFromConnectionString(connectionString);
                var count = namespaceMgr.GetTopic(MappedFileTopicInfo.TopicName).MessageCountDetails.ActiveMessageCount;
                return new List<EnvironmentValidation>()
                {
                    new EnvironmentValidation()
                    {
                        CurrentSetting = "ServiceBusConnection",
                        Name = MappedFileTopicInfo.EnvironmentCheckName,
                        Result = EnvironmentValidationResult.Pass,
                        AdditionalInformation = $"path: {client.Path}, active message count: {count}"
                    }
                };
            }
            catch (Exception e)
            {
                return new List<EnvironmentValidation>()
                {
                    new EnvironmentValidation()
                    {
                        CurrentSetting = "ServiceBusConnection",
                        Name = MappedFileTopicInfo.EnvironmentCheckName,
                        Result = EnvironmentValidationResult.Fail,
                        AdditionalInformation = e.ToString()
                    }
                };
            }
        }
    }
}

