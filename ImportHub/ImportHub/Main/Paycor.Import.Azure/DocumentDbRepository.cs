using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using log4net;

namespace Paycor.Import.Azure
{

    [ExcludeFromCodeCoverage]
    public class DocumentDbRepository<T> : IDocumentDbRepository<T> where T : RepositoryObject
    {
        #region Private Fields

        private string _databaseId;
        private string _collectionId;
        private Database _database;
        private DocumentCollection _collection;
        private DocumentClient _client;
        private readonly ILog _log;

        #endregion

        #region Public Methods

        public DocumentDbRepository(string database, string collection, ILog log)
        {
            Ensure.ThatStringIsNotNullOrEmpty(database, nameof(database));
            Ensure.ThatStringIsNotNullOrEmpty(collection, nameof(collection));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _databaseId = database;
            _collectionId = collection;
            _log = log;
        }

        public DocumentCollection Collection
            => _collection ?? (_collection = ReadOrCreateCollection(Database.SelfLink));

        public DocumentClient Client
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

        public IEnumerable<T> GetItemsFromSystemType(string fullClassName) =>
            Client.CreateDocumentQuery<T>(Collection.DocumentsLink).Where(x => x.SystemType == fullClassName).AsEnumerable();

        #region Get with retries

        public T GetItemWithRetries(Expression<Func<T, bool>> predicate)
            => GetItemsWithRetires(predicate).FirstOrDefault();

        public IEnumerable<T> GetItemsWithRetires(Expression<Func<T, bool>> predicate)
            => GetQueryableItemsWithRetries(predicate).AsEnumerable();

        public IQueryable<T> GetQueryableItemsWithRetries(Expression<Func<T, bool>> predicate)
            =>
                ExecuteWithRetriesGet(
                    () =>
                        Client.CreateDocumentQuery<T>(Collection.DocumentsLink)
                            .Where(predicate));

        #endregion

        public IQueryable<T> GetQueryableItems()
            => Client.CreateDocumentQuery<T>(Collection.DocumentsLink).Where(x => x.SystemType == typeof(T).FullName);

        public IQueryable<T> GetQueryableItems(Expression<Func<T, bool>> predicate)
            => GetQueryableItems().Where(predicate);

        public T GetItem(Expression<Func<T, bool>> predicate)
            => GetItems(predicate).FirstOrDefault();

        public IEnumerable<T> GetItems(Expression<Func<T, bool>> predicate)
            => GetQueryableItems(predicate).AsEnumerable();

        public IEnumerable<T> GetItems()
            => GetQueryableItems().AsEnumerable();

        public async Task<Document> UpdateItemAsync(string id, T item)
        {
            Ensure.ThatStringIsNotNullOrEmpty(id, nameof(id));
            var doc = GetDocument(id);
            item.SystemType = typeof(T).FullName;
            return await ExecuteWithRetries(() => Client.ReplaceDocumentAsync(doc.SelfLink, item));
        }

        public async Task<Document> CreateItemAsync(T item)
        {
            item.SystemType = typeof(T).FullName;
            return await ExecuteWithRetries(() => Client.CreateDocumentAsync(Collection.SelfLink, item));
        }

        public async Task<Document> DeleteItemAsync(string id)
        {
            Ensure.ThatStringIsNotNullOrEmpty(id, nameof(id));
            var doc = GetDocument(id);
            if (null == doc)
            {
                return null;
            }
            return await ExecuteWithRetries(() => Client.DeleteDocumentAsync(doc.SelfLink));
        }

        public async Task<Document> UpsertItemAsync(T item)
        {
            item.SystemType = typeof(T).FullName;
            return await ExecuteWithRetries(() => Client.UpsertDocumentAsync(Collection.DocumentsLink, item));
        }
        #endregion

        #region Protected Methods

        protected virtual DocumentCollection ReadOrCreateCollection(string databaseLink)
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
        #endregion

        #region Private Methods

        private async Task<TV> ExecuteWithRetries<TV>(Func<Task<TV>> function)
        {
            const int requestRateTooLarge = 429;

            while (true)
            {
                TimeSpan sleepTime;
                try
                {
                    return await function();
                }
                catch (DocumentClientException de)
                {
                    if (Convert.ToInt32(de.StatusCode) != requestRateTooLarge)
                    {
                        throw;
                    }
                    _log.Warn("Request Rate too large condition detected. Throttling request.");
                    sleepTime = de.RetryAfter;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is DocumentClientException))
                    {
                        throw;
                    }
                    var de = (DocumentClientException) ae.InnerException;
                    if (Convert.ToInt32(de.StatusCode) != requestRateTooLarge)
                    {
                        throw;
                    }
                    _log.Warn("Request Rate too large condition detected. Throttling request.");
                    sleepTime = de.RetryAfter;
                }
                await Task.Delay(sleepTime);
            }
        }


        private IQueryable<T> ExecuteWithRetriesGet<T>(Func<IQueryable<T>> function)
        {
            const int requestRateTooLarge = 429;
            var count = 0;
            while (true)
            {
                count++;
                if (count > 1)
                {
                    _log.Debug($"Retry happend for {count} times");
                }
                TimeSpan sleepTime;
                try
                {
                    return function();
                }
                catch (DocumentClientException de)
                {
                    if (Convert.ToInt32(de.StatusCode) != requestRateTooLarge)
                    {
                        _log.Debug(
                            $"Error status code from retries is {de.StatusCode}. Throwing back DocumentClientException exception",
                            de);
                        throw;
                    }
                    _log.Warn("Request Rate too large condition detected. Throttling request.");
                    sleepTime = de.RetryAfter;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is DocumentClientException))
                    {
                        _log.Debug("Error status code from retries. Throw back DocumentClientException", ae);
                        throw;
                    }
                    var de = (DocumentClientException) ae.InnerException;
                    if (Convert.ToInt32(de.StatusCode) != requestRateTooLarge)
                    {
                        _log.Debug(
                            $"Error status code from retries is {de.StatusCode}. Throwing back AggregateException", de);
                        throw;
                    }
                    _log.Warn("Request Rate too large condition detected. Throttling request.");
                    sleepTime = de.RetryAfter;
                }
                catch (Exception ex)
                {
                    _log.Error($"Some Other Exception occurred at {nameof(ExecuteWithRetriesGet)}", ex);
                    throw;
                }
                Task.Delay(sleepTime);
                _log.Debug($"Waited for retries with Get for {sleepTime}");
            }
        }

        private Document GetDocument(string id)
        {
            return Client.CreateDocumentQuery(Collection.DocumentsLink)
                .Where(d => d.Id == id)
                .AsEnumerable()
                .FirstOrDefault();
        }

        private Database ReadOrCreateDatabase()
        {
            var db = Client.CreateDatabaseQuery()
                .Where(d => d.Id == DatabaseId)
                .AsEnumerable()
                .FirstOrDefault() ?? Client.CreateDatabaseAsync(new Database {Id = DatabaseId}).Result;

            return db;
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
