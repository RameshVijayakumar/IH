using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Paycor.Import.Mapping;

namespace Paycor.Import.Azure
{
    public class TableStorageProvider : ITableStorageProvider
    {
        private readonly ILog _log;
        private string _tableConnectionString;
        private const string TableName = "MappingAudit";
        private CloudTable _table;
        private CloudStorageAccount _account;
        private CloudTableClient _client;

        public TableStorageProvider(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
        }
        private string TableConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_tableConnectionString))
                {
                    var tableConnectionString = ConfigurationManager.AppSettings["StorageConnection"];
                    if (string.IsNullOrEmpty(tableConnectionString))
                    {
                        throw new Exception("StorageConnection is not configured");
                    }
                    _tableConnectionString = tableConnectionString;
                }

                return _tableConnectionString;
            }
        }
        public void Insert(ApiMappingAudit item)
        {
            Ensure.ThatArgumentIsNotNull(item, nameof(item));
            try
            {
                var operation = TableOperation.Insert(item);
                if (_table == null || !_table.Exists())
                {
                    GetTable(true);
                }
                // ReSharper disable once PossibleNullReferenceException
                // Expectation is that AccessTable will ensure that _table is not null.
                _table.Execute(operation);
            }
            catch (Exception ex)
            {                
               _log.Error($"Error occurred while inserting into table storage {TableName}", ex);
            }
            
        }

        public IEnumerable<ApiMappingAudit> GetItems(string mapName)
        {
            Ensure.ThatStringIsNotNullOrEmpty(mapName, nameof(mapName));
            if (_table == null)
            {
                GetTable(false);
            }
            var query =
                    new TableQuery<ApiMappingAudit>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
                        QueryComparisons.Equal, mapName.ToLower()));

            // ReSharper disable once PossibleNullReferenceException
            // Expectation is that AccessTable will ensure that _table is not null.
            return _table.ExecuteQuery(query).ToList();          
        }


        private void GetTable(bool createIfNotExists)
        {
            _account = CloudStorageAccount.Parse(TableConnectionString);
            _client = _account.CreateCloudTableClient();
            _table = _client.GetTableReference(TableName);
            if (createIfNotExists) _table.CreateIfNotExists();
        }
    }
}
