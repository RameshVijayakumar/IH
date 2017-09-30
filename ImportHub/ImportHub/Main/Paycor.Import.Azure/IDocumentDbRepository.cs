using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using System.Collections.Generic;

namespace Paycor.Import.Azure
{
    public interface IDocumentDbRepository<T> : IRepository<T, Document> where T : RepositoryObject

    {
        Task<Document> UpsertItemAsync(T item);

        IQueryable<T> GetQueryableItems(Expression<Func<T, bool>> predicate);

        IQueryable<T> GetQueryableItems();

        IEnumerable<T> GetItemsFromSystemType(string fullClassName);
    }
}