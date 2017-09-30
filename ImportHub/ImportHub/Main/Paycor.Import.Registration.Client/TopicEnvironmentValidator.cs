using System;
using System.Collections.Generic;
using Paycor.SystemCheck;
using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Paycor.Import.Registration.Client
{
    public class TopicEnvironmentValidator : IEnvironmentValidator
    {
        public IEnumerable<EnvironmentValidation> EnvironmentValidate()
        {
            try
            {
                var connectionString = ConfigurationManager.AppSettings[RegistrationServiceTopicInfo.ServiceBusConnectionKey];
                var client = TopicClient.CreateFromConnectionString(connectionString, RegistrationServiceTopicInfo.TopicName);
                var namespaceMgr = NamespaceManager.CreateFromConnectionString(connectionString);
                var count = namespaceMgr.GetTopic(RegistrationServiceTopicInfo.TopicName).MessageCountDetails.ActiveMessageCount;
                return new List<EnvironmentValidation>()
                {
                    new EnvironmentValidation()
                    {
                        CurrentSetting = RegistrationServiceTopicInfo.ServiceBusConnectionKey,
                        Name = "Registration Topic",
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
                        CurrentSetting = RegistrationServiceTopicInfo.ServiceBusConnectionKey,
                        Name = "Registration Topic",
                        Result = EnvironmentValidationResult.Fail,
                        AdditionalInformation = e.ToString()
                    }
                };
            }
        }
    }
}


