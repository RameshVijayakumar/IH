using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paycor.Import.Status
{
    /// <summary>
    /// Defines methods necessary to implement a status storage
    /// provider. Status storage providers are used by the <see cref="StatusManager{T}"/> 
    /// to save status messages for future retrieval by the <see cref="BaseStatusReceiver{T}"/>.
    /// </summary>
    public interface IStatusStorageProvider
    {
        /// <summary>
        /// Stores a status message using the implemented storage strategy.
        /// </summary>
        /// <param name="statusMessage">the status message to store</param>
        void StoreStatus(StatusMessage statusMessage);
        
        /// <summary>
        /// Retrieves a status message using the implemented storage strategy.
        /// </summary>
        /// <param name="reporter">the identity of the reporter to get status from</param>
        /// <param name="key">the key which uniquely identifies a given status entry</param>
        /// <returns>the status message associated with the reporter and key combination</returns>
        StatusMessage RetrieveStatus(string reporter, string key);

        /// <summary>
        /// Deletes a status message using the implemented storage strategy.
        /// </summary>
        /// <param name="reporter">the identity of the reporter to get status from</param>
        /// <param name="keys">the keys which uniquely identifies given status entries</param>
        Task DeleteStatusAsync(string reporter, IEnumerable<string> keys);
    }
}
