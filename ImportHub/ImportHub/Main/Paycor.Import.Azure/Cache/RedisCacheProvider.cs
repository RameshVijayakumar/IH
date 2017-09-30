using System;
using log4net;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Paycor.Import.Azure.Cache
{
    public class RedisCacheProvider<T> : ICacheProvider<T>
    {
        private readonly IDatabase _cache;
        private readonly ILog _log;
        private readonly bool _isRedisConnected;
        private readonly string _keyPrefix;

        public RedisCacheProvider(IDatabase cache, ILog log, bool isRedisConnected)
        {
            _cache = cache;
            _log = log;
            _isRedisConnected = isRedisConnected;
            _keyPrefix = typeof(T).Name;
        }

        public bool Store(T item, string key, TimeSpan? expiry = null)
        {
            try
            {
                _log.Debug($"#rediscache stored an item of {_keyPrefix}.");
                return _cache.StringSet(GenerateFullKey(key), JsonConvert.SerializeObject(item), expiry);
            }
            catch (Exception ex)
            {
                _log.Warn($"An error occurred while storing item of {_keyPrefix} in #rediscache.", ex);
                return false;
            }
        }

        public T Retrieve(string key)
        {
            try
            {
                var fullKey = GenerateFullKey(key);
                if (!_cache.KeyExists(fullKey))
                {
                    _log.Debug($"The requested cache item of {_keyPrefix} does not exist in #rediscache.");
                    return default(T);
                }

                _log.Debug($"#rediscache has found an item of {_keyPrefix} and is retrieving it from cache.");
                return JsonConvert.DeserializeObject<T>(_cache.StringGet(fullKey));
            }
            catch (Exception ex)
            {
                _log.Error($"An error occurred while Retrieving item of {key} in #rediscache.", ex);
                return default(T);
            }
        }

        public void Remove(string key)
        {
            var fullKey = GenerateFullKey(key);
            if (!_cache.KeyExists(fullKey))
            {
                _log.Debug($"The requested cache item of {_keyPrefix} does not exist in #rediscache.");
                return;
            }
            _cache.KeyDelete(fullKey);
            _log.Debug($"#rediscache has found an item of {_keyPrefix} and has removed it from cache.");
        }

        public bool IsConnected()
        {
            return _isRedisConnected;
        }

        private string GenerateFullKey(string key) => $"{_keyPrefix}_{key}";
    }
}