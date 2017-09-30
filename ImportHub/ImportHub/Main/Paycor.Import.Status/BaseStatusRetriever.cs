namespace Paycor.Import.Status
{
    /// <summary>
    /// This class is responsible for retrieving status information from an
    /// <see cref="IStatusStorageProvider" /> and returning it to the caller.
    /// </summary>
    /// <typeparam name="T">
    /// The type of data the caller expectes to receive when calling RetrieveStatus.
    /// </typeparam>
    public abstract class BaseStatusRetriever<T> : IStatusRetriever<T>
    {
        private IStatusStorageProvider _statusStorageProvider;
        private string _reporter;

        /// <summary>
        /// Constructs a new instance of the <see cref="BaseStatusReceiver{T}"/> class.
        /// </summary>
        /// <param name="statusStorageProvider">the status storage provider associated with this retriever</param>
        /// <param name="reporter">the reporter associated with this receiver</param>
        protected BaseStatusRetriever(IStatusStorageProvider statusStorageProvider, string reporter)
        {
            Ensure.ThatArgumentIsNotNull(statusStorageProvider, nameof(statusStorageProvider));
            Ensure.ThatStringIsNotNullOrEmpty(reporter, nameof(reporter));

            _statusStorageProvider = statusStorageProvider;
            _reporter = reporter;
        }

        /// <summary>
        /// Retrieves the status from the registered <see cref="IStatusStorageProvider"/>
        /// </summary>
        /// <param name="key">the unique identifying key associated with the status to retrieve</param>
        /// <returns>the found status deserialized as {T}, null otherwise</returns>
        public T RetrieveStatus(string key)
        {
            Ensure.ThatStringIsNotNullOrEmpty(key, nameof(key));
            var statusMessage = _statusStorageProvider.RetrieveStatus(_reporter, key);
            return Deserialize(statusMessage);
        }

        /// <summary>
        /// Provided to allow the implementor to deserialize from storage using
        /// their preferred deserializer.
        /// </summary>
        /// <param name="message">the message to deserialize</param>
        /// <returns>the deserialized status</returns>
        protected abstract T Deserialize(StatusMessage message);
    }
}
