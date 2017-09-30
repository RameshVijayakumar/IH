namespace Paycor.Import.Status
{
    /// <summary>
    /// This class is responsible for receiving status messages and passing
    /// them on to the appropriate storage provider.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the status message the manager is designed to handle.
    /// </typeparam>
    public class StatusManager<T> 
    {
        private readonly IStatusStorageProvider _statusStorageProvider;

        public StatusManager(IStatusReceiver<T> statusReceiver, IStatusStorageProvider statusStorageProvider)
        {
            Ensure.ThatArgumentIsNotNull(statusReceiver, nameof(statusReceiver));
            Ensure.ThatArgumentIsNotNull(statusStorageProvider, nameof(statusStorageProvider));

            _statusStorageProvider = statusStorageProvider;

            statusReceiver.StatusReceived += StatusReceiverOnStatusReceived;
        }

        private void StatusReceiverOnStatusReceived(object sender, StatusMessage statusMessage)
        {
            Ensure.ThatArgumentIsNotNull(statusMessage, nameof(statusMessage));
            _statusStorageProvider.StoreStatus(statusMessage);
        }
    }
}
