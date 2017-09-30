using System;
using System.Configuration;
using System.Web.UI;
using Microsoft.ServiceBus.Messaging;

namespace Paycor.Import.Registration.Client
{
    public class WebApiStartup
    {
        public static void RegisterServiceApi()
        {
            var connectionString = ConfigurationManager.AppSettings[RegistrationServiceTopicInfo.ServiceBusConnectionKey];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"Please provide a valid AppSetting value for the service bus connection string key: {RegistrationServiceTopicInfo.ServiceBusConnectionKey}.");
            }
            RegisterServiceApi(connectionString);
        }

        public static void RegisterServiceApi(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString", "The service bus connection string is either null or empty. Please provide a valid connection to the service bus.");
            }

            var swaggerDocUrls = ConfigurationManager.AppSettings[RegistrationServiceTopicInfo.SwaggerDocVersionKey];
            if (string.IsNullOrEmpty(swaggerDocUrls))
            {
                throw new Exception($"Please provide a valid AppSetting value for the swagger doc registration Urls: {RegistrationServiceTopicInfo.SwaggerDocVersionKey}.");
            }

            var urls = swaggerDocUrls.Split(';');
            var client = TopicClient.CreateFromConnectionString(connectionString, RegistrationServiceTopicInfo.TopicName);

            //TODO: It would be much more preferrable to get the swagger docs from the swagger configuration, but it is not currently known how to get this information at start up.
            foreach (var url in urls)
            {
                client.Send(new BrokeredMessage(url));
            }
        }
    }
}
