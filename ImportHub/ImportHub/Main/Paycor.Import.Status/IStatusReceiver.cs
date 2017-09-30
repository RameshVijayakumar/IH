using System;

namespace Paycor.Import.Status
{
    /// <summary>
    /// Defines methods that need to be implemented when creating
    /// a status receiver. A status receiver gets messages from
    /// workers and submits them to the status manager, where the message
    /// will be processed and handed off to the approriate <see cref="IStatusStorageProvider"/> 
    /// implementation.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the status message the reciever is designed to handle.
    /// </typeparam>
    public interface IStatusReceiver<T>
    {
        /// <summary>
        /// Raised when a new message is sent by the <see cref="IStatusReceiver{T}"/>.
        /// </summary>
        event EventHandler<StatusMessage> StatusReceived;

        /// <summary>
        /// Sends a status message to the <see cref="StatusManager{T}"/>.
        /// </summary>
        /// <param name="key">the unique component of the status key</param>
        /// <param name="statusType">the type of the status to send</param>
        /// <param name="reporter">allows the reporter to be sent in "pass thru" mode</param>
        void Send(string key, T statusType, string reporter = null);
    }
}
