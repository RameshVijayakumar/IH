using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;


namespace Paycor.Import.Azure.Adapter
{
    /// <summary>
    /// Sends messages of type T to the specified Azure Service Bus Topic. Since there are no
    /// state variables for this class, it can be injected as a singleton class.
    /// </summary>
    /// <typeparam name="T">the type of the message to send to the topic</typeparam>
    public class ServiceBusTopicClient<T> : BaseServiceBusClient<T>
    {
        public ServiceBusTopicClient(int retries = DefaultRetries, int millisecondsToWait = DefaultWait) : base(retries, millisecondsToWait) { }

        /// <summary>
        /// Send a message to the specified Azure Service Bus topic using the specified Azure service bus account.
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="topic">the name of the topic</param>
        /// <param name="serviceBusConnectionString">the service bus account connection string</param>
        protected override void SendMessageImplementation(T message, string topic, string serviceBusConnectionString)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
            if (!namespaceManager.TopicExists(topic))
            {
                namespaceManager.CreateTopic(topic);
            }
            var client = TopicClient.CreateFromConnectionString(serviceBusConnectionString, topic);

            client.Send(new BrokeredMessage(message));
        }
    }
}
