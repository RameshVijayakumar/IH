using System;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
using Microsoft.Practices.TransientFaultHandling;

namespace Paycor.Import.Azure.Adapter
{
    public abstract class BaseServiceBusClient<T> : ICloudMessageClient<T>
    {
        public const int DefaultRetries = 5;
        public const int DefaultWait = 3000;

        private readonly int _retries;
        private readonly int _millisecondsToWait;

        protected BaseServiceBusClient(int retries = DefaultRetries, int millisecondsToWait = DefaultWait)
        {
            _retries = retries;
            _millisecondsToWait = millisecondsToWait;
        }

        public void SendMessage(T message, string queue, string serviceBusConnectionString)
        {
            var retryPolicy = new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(_retries, new TimeSpan(0, 0, 0, 0, _millisecondsToWait));
            retryPolicy.ExecuteAction(
                () => SendMessageImplementation(message, queue, serviceBusConnectionString));
        }

        protected abstract void SendMessageImplementation(T message, string queue, string serviceBusConnectionString);
    }
}
