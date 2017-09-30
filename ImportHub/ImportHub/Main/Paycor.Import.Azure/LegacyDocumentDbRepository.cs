using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Paycor.Import.Azure
{
    public class LegacyDocumentDbRepository<T> where T:class
    {
        #region Private Fields

        private string _databaseId;
        private string _collectionId;
        private Database _database;
        private DocumentCollection _collection;
        private DocumentClient _client;

        #endregion

        #region Public Methods

        public LegacyDocumentDbRepository(string database, string collection)
        {
            Ensure.ThatStringIsNotNullOrEmpty(database, nameof(database));
            Ensure.ThatStringIsNotNullOrEmpty(collection, nameof(collection));
            _databaseId = database;
            _collectionId = collection;
        }

        private DocumentCollection Collection
            => _collection ?? (_collection = ReadOrCreateCollection(Database.SelfLink));

        private DocumentClient Client
        {
            get
            {
                if (_client == null)
                {
                    string endpoint = ConfigurationManager.AppSettings["endpoint"];
                    string authKey = ConfigurationManager.AppSettings["authKey"];
                    if (string.IsNullOrEmpty(endpoint))
                        throw new Exception("Missing AppSetting key of endpoint from config file.");
                    if (string.IsNullOrEmpty(authKey))
                        throw new Exception("Missing AppSetting key of authkey from config file.");

                    Uri endpointUri = new Uri(endpoint);
                    _client = new DocumentClient(endpointUri, authKey);
                }

                return _client;
            }
        }

        public IEnumerable<T> GetItems()
            => GetQueryableItems().AsEnumerable();

        #endregion

        #region Private Methods

        private IQueryable<T> GetQueryableItems()
            => Client.CreateDocumentQuery<T>(Collection.DocumentsLink);

        private Database ReadOrCreateDatabase()
        {
            var db = Client.CreateDatabaseQuery()
                .Where(d => d.Id == DatabaseId)
                .AsEnumerable()
                .FirstOrDefault() ?? Client.CreateDatabaseAsync(new Database { Id = DatabaseId }).Result;

            return db;
        }

        private DocumentCollection ReadOrCreateCollection(string databaseLink)
        {
            var col = Client.CreateDocumentCollectionQuery(databaseLink)
                .Where(c => c.Id == CollectionId)
                .AsEnumerable()
                .FirstOrDefault();


            if (col == null)
            {
                var collectionSpec = new DocumentCollection { Id = CollectionId };
                var requestOptions = new RequestOptions { OfferType = "S1" };

                col = Client.CreateDocumentCollectionAsync(databaseLink, collectionSpec, requestOptions).Result;
            }
            return col;
        }

        private string DatabaseId
        {
            get
            {
                if (string.IsNullOrEmpty(_databaseId))
                {
                    _databaseId = ConfigurationManager.AppSettings["database"];
                }

                return _databaseId;
            }
        }

        private string CollectionId
        {
            get
            {
                if (string.IsNullOrEmpty(_collectionId))
                {
                    _collectionId = ConfigurationManager.AppSettings["collection"];
                }

                return _collectionId;
            }
        }

        private Database Database => _database ?? (_database = ReadOrCreateDatabase());

        #endregion
    }
}
