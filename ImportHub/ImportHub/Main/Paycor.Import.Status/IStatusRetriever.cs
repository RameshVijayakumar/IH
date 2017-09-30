namespace Paycor.Import.Status
{
    /// <summary>
    /// Provides an interface for retrieving status information from an
    /// <see cref="IStatusStorageProvider" /> and returning it to the caller.
    /// </summary>
    /// <typeparam name="T">
    /// The type of data the caller expectes to receive when calling RetrieveStatus.
    /// </typeparam>
    public interface IStatusRetriever<T>
    {
        /// <summary>
        /// Retrieves the status from the registered <see cref="IStatusStorageProvider"/>
        /// </summary>
        /// <param name="key">the unique identifying key associated with the status to retrieve</param>
        /// <returns>the found status deserialized as {T}, null otherwise</returns>
        T RetrieveStatus(string key);
    }
}