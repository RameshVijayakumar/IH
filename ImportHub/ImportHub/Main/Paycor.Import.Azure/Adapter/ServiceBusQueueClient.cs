using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Paycor.Import.Azure.Adapter
{
    /// <summary>
    /// Sends messages of type T to the specified Azure Service Bus Queue. Since there are no
    /// state variables for this class, it can be injected as a singleton class.
    /// </summary>
    /// <typeparam name="T">the type of the message to send to the queue</typeparam>
    public class ServiceBusQueueClient<T> : BaseServiceBusClient<T>
    {
        public ServiceBusQueueClient(int retries = DefaultRetries, int millisecondsToWait = DefaultWait) : base(retries, millisecondsToWait) { }

        protected override void SendMessageImplementation(T message, string queue, string serviceBusConnectionString)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
            if (!namespaceManager.QueueExists(queue))
            {
                namespaceManager.CreateQueue(queue);
            }
            var client = QueueClient.CreateFromConnectionString(serviceBusConnectionString, queue);

            client.Send(new BrokeredMessage(message));
        }
    }
}
