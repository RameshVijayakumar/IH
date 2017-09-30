using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Paycor.Import.Messaging;

namespace Paycor.Import.Azure
{
    public class ImportCancelTokenStorage : IStoreData<ImportCancelToken>
    {
        private CloudStorageAccount _account;
        private CloudTableClient _client;
        private CloudTable _table;
        private readonly string _tableName;
        private readonly string _storageConnection;
        private readonly string _partitionKey;

        public ImportCancelTokenStorage(string storageConnection, string tableName, string partitionKey)
        {
            Ensure.ThatStringIsNotNullOrEmpty(storageConnection, nameof(storageConnection));
            Ensure.ThatStringIsNotNullOrEmpty(tableName, nameof(tableName));
            Ensure.ThatArgumentIsNotNull(partitionKey, nameof(partitionKey));
            _storageConnection = storageConnection;
            _tableName = tableName;
            _partitionKey = partitionKey;
        }

        public ImportCancelToken Retrieve(string rowKey)
        {
            Ensure.ThatArgumentIsNotNull(rowKey, nameof(rowKey));

            if (_table == null) AccessTable(true);

            var partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey);
            var rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);
            var query = new TableQuery<ImportCancelTokenEntity>()
                .Where(TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter));

            var result = _table?.ExecuteQuery(query).SingleOrDefault();
            return result?.AsImportCancelToken();
        }

        public void Store(ImportCancelToken cancelToken)
        {
            Ensure.ThatArgumentIsNotNull(cancelToken, nameof(cancelToken));
            var tableEntity = new ImportCancelTokenEntity
            {
                CancelRequested = cancelToken.CancelRequested,
                PartitionKey = _partitionKey,
                RowKey = cancelToken.TransactionId
            };
            var insertOperation = TableOperation.InsertOrReplace(tableEntity);
            if (_table == null) AccessTable(true);

            _table?.Execute(insertOperation);
        }

        private void AccessTable(bool createIfNotExists)
        {
            _account = CloudStorageAccount.Parse(_storageConnection);
            _client = _account.CreateCloudTableClient();
            _table = _client.GetTableReference(_tableName);
            if (createIfNotExists) _table.CreateIfNotExists();
        }
    }
}