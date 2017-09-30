using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Paycor.Import.Azure;

namespace Paycor.Import.Status.Azure
{
    public class TableStorageStatusStorageProvider : IStatusStorageProvider
    {

        private CloudStorageAccount _account;
        private CloudTableClient _client;
        private CloudTable _table;
        private readonly string _tableName;

        public TableStorageStatusStorageProvider(string tableName)
        {
            Ensure.ThatStringIsNotNullOrEmpty(tableName, nameof(tableName));

            _tableName = tableName;
        }

        public Task DeleteStatusAsync(string reporter, IEnumerable<string> keys)
        {
            // YAGNI - not needed for now.
            throw new NotImplementedException();
        }

        public StatusMessage RetrieveStatus(string reporter, string key)
        {
            Ensure.ThatStringIsNotNullOrEmpty(reporter, nameof(reporter));
            Ensure.ThatStringIsNotNullOrEmpty(key, nameof(key));

            if (_table == null)
            {
                AccessTable(false);
            }

            var partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, reporter);
            var rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, key);
            var query = new TableQuery<TableStorageStatusEntity>()
                .Where(TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter));

            // ReSharper disable once PossibleNullReferenceException
            // Expectation is that AccessTable will ensure that _table is not null.
            var result = _table.ExecuteQuery(query).SingleOrDefault();

            return result != null ? result.AsStatusMessage() : null;
        }

        public void StoreStatus(StatusMessage statusMessage)
        {
            Ensure.ThatArgumentIsNotNull(statusMessage, nameof(statusMessage));
            Ensure.ThatPropertyIsInitialized(statusMessage.Reporter, nameof(statusMessage.Reporter));
            Ensure.ThatPropertyIsInitialized(statusMessage.Key, nameof(statusMessage.Key));

            var tableEntity = new TableStorageStatusEntity(statusMessage);
            var insertOperation = TableOperation.InsertOrReplace(tableEntity);
            if (_table == null)
            {
                AccessTable(true);
            }
            
            // ReSharper disable once PossibleNullReferenceException
            // Expectation is that AccessTable will ensure that _table is not null.
            _table.Execute(insertOperation);
        }

        protected void AccessTable(bool createIfNotExists)
        {
            _account = CloudStorageAccount.Parse(AzureConfiguration.StorageConnectionString);
            _client = _account.CreateCloudTableClient();
            _table = _client.GetTableReference(_tableName);
            if (createIfNotExists) _table.CreateIfNotExists();
        }
    }
}
