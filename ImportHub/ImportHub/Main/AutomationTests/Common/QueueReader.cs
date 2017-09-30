using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Paycor.Import.ImportHubTest.Common
{
    public class QueueReader<T> : BaseServiceBusReader<T>
    {
        public QueueReader(string connectionString, string path) : base(connectionString, path)
        {
        }

        public override T ReceiveMessage()
        {
            var nsm = NamespaceManager.CreateFromConnectionString(ConnectionString);
            if (!nsm.QueueExists(Path))
            {
                throw new AutomationTestException($"The queue at {Path} does not exist.");
            }
            var client = QueueClient.CreateFromConnectionString(ConnectionString, Path, ReceiveMode.ReceiveAndDelete);
            var message = client.Receive();
            var body = message.GetBody<T>();
            message.Complete();
            return body;
        }
    }
}
