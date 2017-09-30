using System;

namespace Paycor.Import
{
    public interface ICacheProvider<T>
    {
        bool Store(T item, string key, TimeSpan? expiry = null);

        T Retrieve(string key);

        void Remove(string key);

        bool IsConnected();
    }
}
