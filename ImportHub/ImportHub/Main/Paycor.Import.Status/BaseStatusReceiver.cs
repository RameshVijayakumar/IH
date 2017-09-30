using System;

namespace Paycor.Import.Status
{
    /// <summary>
    /// Abstract class which defines the base behavior for an IStatusReceiver instance.
    /// </summary>
    /// <typeparam name="T">The type of the status that is received by this class.</typeparam>
    public abstract class BaseStatusReceiver<T> : IStatusReceiver<T>
    {
        public event EventHandler<StatusMessage> StatusReceived;

        private readonly string _reporter;

        /// <summary>
        /// The preferred constructor. Allows the application to register an
        /// event "reporter" so that any status updates made with this receiver
        /// are automatically associated with the registered reporter.
        /// </summary>
        /// <param name="reporter">the reporter to register</param>
        protected BaseStatusReceiver(string reporter)
        {
            Ensure.ThatStringIsNotNullOrEmpty(reporter, nameof(reporter));
            _reporter = reporter;
        }

        protected BaseStatusReceiver()
        {

        }

        /// <summary>
        /// Sends a message to the <see cref="StatusManager{T}"/> that can be stored by an 
        /// <see cref="IStatusStorageProvider"/>.
        /// </summary>
        /// <param name="key">the key to identify the status message</param>
        /// <param name="statusType">the type of the status message to send</param>
        /// <param name="reporter">the reporter (used in Service Bus "pass thru" only)</param>
        public void Send(string key, T statusType, string reporter = null)
        {
            var selectedReporter = string.IsNullOrEmpty(reporter) ? _reporter : reporter;
            var status = Serialize(statusType);
            OnSend(new StatusMessage
            {
                Reporter = selectedReporter,
                Key = key,
                Status = status
            });
        }

        /// <summary>
        /// Provides a means to serialize the specified type to a string so that it may be stored
        /// by the <see cref="IStatusStorageProvider"/>.
        /// </summary>
        /// <param name="statusType">the type of the class to serialize</param>
        /// <returns>a string representing the serialized class for storage</returns>
        protected abstract string Serialize(T statusType);

        /// <summary>
        /// Method that provides a convenience method to raise the <see cref="StatusReceived"/> event.
        /// </summary>
        /// <param name="message">the message to process</param>
        protected virtual void OnSend(StatusMessage message)
        {
            var handler = StatusReceived;
            if (handler != null)
            {
                handler(this, message);
            }
        }
    }
}
