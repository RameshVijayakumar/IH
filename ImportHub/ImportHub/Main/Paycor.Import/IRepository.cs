using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Paycor.Import
{
    public interface IRepository<T, TResult> where T : RepositoryObject
    {
        T GetItem(Expression<Func<T, bool>> predicate);
        IEnumerable<T> GetItems(Expression<Func<T, bool>> predicate);
        IEnumerable<T> GetItems();
        Task<TResult> UpdateItemAsync(string id, T item);
        Task<TResult> CreateItemAsync(T item);
        Task<TResult> DeleteItemAsync(string id);
        T GetItemWithRetries(Expression<Func<T, bool>> predicate);
    }
}
