using log4net;
using Microsoft.Azure.Documents;
using System.Collections.ObjectModel;
using System.Linq;


namespace Paycor.Import.Azure
{
    public class IndexDocumentDbRepository<T> : DocumentDbRepository<T> where T : RepositoryObject
    {
        private readonly IndexingPolicy _indexingPolicy;

        public IndexDocumentDbRepository(IndexingPolicy indexingPolicy, string database, string collectionId, ILog log) : base(database, collectionId, log)
        {
            _indexingPolicy = indexingPolicy;
        }

        protected override DocumentCollection ReadOrCreateCollection(string databaseLink)
        {
            var collection = base.ReadOrCreateCollection(databaseLink);
            var paths = _indexingPolicy.IncludedPaths;
            var pathExists = IsPathExists(paths, collection);

            if(!pathExists)
            {
                collection.IndexingPolicy = _indexingPolicy;
            }
            return collection;                   
        }

        private static bool IsPathExists(Collection<IncludedPath> paths, DocumentCollection collection)
        {
            var hasPath = false;
            foreach(var path in paths)
            {
                hasPath = collection.IndexingPolicy.IncludedPaths.Any(x => x.Path == path.ToString());
            }
            return hasPath;
        }
    }
}
