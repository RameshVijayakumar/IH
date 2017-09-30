
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paycor.Import.Status.Azure
{
    /// <summary>
    /// Provides a class to store messages to an Azure Service Bus Queue. Note that 
    /// this class currently does not support the ability to get messages from the
    /// service bus queue since web jobs have an implicit method to retrieve messages.
    /// </summary>
    public class ServiceBusStatusStorageProvider : IStatusStorageProvider
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public ServiceBusStatusStorageProvider(string connectionString, string queueName)
        {
            Ensure.ThatStringIsNotNullOrEmpty(connectionString, nameof(connectionString));
            Ensure.ThatStringIsNotNullOrEmpty(queueName, nameof(queueName));

            _connectionString = connectionString;
            _queueName = queueName;
        }

        public Task DeleteStatusAsync(string reporter, IEnumerable<string> keys)
        {
            // YAGNI - not needed for now.
            throw new NotImplementedException();
        }

        public StatusMessage RetrieveStatus(string reporter, string key)
        {
            // YAGNI - not needed for now.
            throw new NotImplementedException();
        }

        public void StoreStatus(StatusMessage statusMessage)
        {
            Ensure.ThatArgumentIsNotNull(statusMessage, nameof(statusMessage));

            var client = QueueClient.CreateFromConnectionString(_connectionString, _queueName);
            client.Send(new BrokeredMessage(statusMessage));
        }
    }
}
