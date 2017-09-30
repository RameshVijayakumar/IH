using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.Messaging;

namespace Paycor.Import
{
    public class Cookie
    {
        public string CookieValue { get; set; }
    }
    public class CookieResolver : ICookieResolver
    {
        private readonly IHttpInvoker _httpInvoker;
        private readonly string _accountsEndpoint;
        private readonly ILog _log;
        private readonly IDictionary<string,string> _headers;
        private readonly ICacheProvider<Cookie> _redisCache;
        private static readonly TimeSpan CookieTimeSpan = TimeSpan.FromHours(12); 

        public CookieResolver(ILog log,
            ICacheProvider<Cookie> redisCache,
            IHttpInvoker httpInvoker, string accountsEndpoint, 
            IDictionary<string,string> headers)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(httpInvoker, nameof(httpInvoker));
            Ensure.ThatStringIsNotNullOrEmpty(accountsEndpoint, nameof(accountsEndpoint));
            Ensure.ThatArgumentIsNotNull(redisCache, nameof(redisCache));

            _httpInvoker = httpInvoker;
            _accountsEndpoint = accountsEndpoint;
            _log = log;
            _headers = headers;
            _redisCache = redisCache;
        }

        public async Task<string> ResolveAsync(string apiKey, string apiSecretKey)
        {
            try
            {
                if (!IsValid(apiKey, apiSecretKey))
                    return string.Empty;

                var key = apiKey + apiSecretKey;
                var cookieValue = RetrieveCookieFromCache(key);
                if (!string.IsNullOrWhiteSpace(cookieValue))
                {
                    _log.Debug("Retrieved Cookie From Cache");
                    return cookieValue;
                }

                cookieValue = await RetrieveCookieFromSecurityApi(apiKey, apiSecretKey);

                if (!string.IsNullOrWhiteSpace(cookieValue) && _redisCache.IsConnected())
                {
                    _redisCache.Store(new Cookie { CookieValue = cookieValue }, key, CookieTimeSpan);
                }

                return cookieValue;

            }
            catch (Exception ex)
            {
                _log.Error("An error occurred while resolving the cookie from SMA.", ex);
                return string.Empty;
            }
        }

        private string RetrieveCookieFromCache(string key)
        {
            if (!_redisCache.IsConnected()) return string.Empty;
            var cookie = _redisCache.Retrieve(key);
            return !string.IsNullOrWhiteSpace(cookie?.CookieValue) ? cookie.CookieValue : string.Empty;
        }

        private async Task<string> RetrieveCookieFromSecurityApi(string apiKey, string apiSecretKey)
        {
            var response = await _httpInvoker.CallApiEndpointWithApiKeyAsync(apiKey, apiSecretKey,
                null, _accountsEndpoint, HtmlVerb.Get, _headers);

            var cookieValues = response.Response.Headers.GetValues("Set-Cookie");

            var cookie = cookieValues.GetPayCorAuthCookie();

            if (string.IsNullOrWhiteSpace(cookie))
                _log.Warn("Value of PaycorAuth cookie is empty");

            _log.Debug("Retrieved Cookie From Security Api");
            return cookie;
        }

        public bool IsValid(string apiKey, string apiSecretKey)
        {
            if (string.IsNullOrWhiteSpace(apiSecretKey))
            {
                _log.Warn("ApisecretKey is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _log.Warn("ApiKey is empty");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_accountsEndpoint)) return true;
            _log.Warn("Accounts Endpoint is empty");
            return false;
        }
    }
}