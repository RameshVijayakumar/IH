using Microsoft.WindowsAzure.Storage.Table;

namespace Paycor.Import.Status.Azure
{
    internal class TableStorageStatusEntity : TableEntity
    {
        public TableStorageStatusEntity()
        {
            
        }

        public TableStorageStatusEntity(string reporter, string key)
        {
            PartitionKey = reporter;
            RowKey = key;
        }

        public TableStorageStatusEntity(string reporter, string key, string status)
            : this(reporter, key)
        {
            Status = status;
        }

        public TableStorageStatusEntity(StatusMessage message)
            : this(message.Reporter, message.Key, message.Status)
        {
            Ensure.ThatArgumentIsNotNull(message, nameof(message));
        }

        public StatusMessage AsStatusMessage()
        {
            return new StatusMessage
            {
                Reporter = Reporter,
                Key = Key,
                Status = Status
            };
        }

        public string Reporter { get { return PartitionKey; } }
        public string Key { get { return RowKey; } }
        public string Status { get; set; }
    }
}
