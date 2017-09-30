using System;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Paycor.Import.ImportHubTest.Common
{
    public class TopicReader<T> : BaseServiceBusReader<T>
    {
        private readonly string _subscription;

        public TopicReader(string connectionString, string path, string subscription) : base(connectionString, path)
        {
            _subscription = subscription;
        }

        public override T ReceiveMessage()
        {
            var nsm = NamespaceManager.CreateFromConnectionString(ConnectionString);
            if (!nsm.SubscriptionExists(Path, _subscription))
            {
                nsm.CreateSubscription(Path, _subscription);
            }

            var client = SubscriptionClient.CreateFromConnectionString(ConnectionString, Path, _subscription);

            var message = client.Receive();
            var body = message.GetBody<T>();
            message.Complete();
            client.Close();
            
            return body;
        }
    }
}


/* 
 * Notes for Weichen:
 * 
 * var topicClient = new TopicClient<Paycor.Import.Messaging.FileImportEventMessage>(connectionstring, "importhubevents", "testSubscription");
 * FileImportEventMessage message = ReceiveMessage();
 * 
 * https://azure.microsoft.com/en-us/documentation/articles/service-bus-dotnet-how-to-use-topics-subscriptions/
 */
