using System;
using System.Net.Http;
//TODO: No unit tests

namespace Paycor.Import.Web
{
    public class ApiCookieHttpClient
    {
        private readonly string _baseAddress;
        private readonly Guid _masterSessionId;
        private readonly int _timeout;

        public ApiCookieHttpClient(string baseAddress, Guid masterSessionId, int timeout = ImportConstants.DefaultHttpTimeout)
        {
            Ensure.ThatStringIsNotNullOrEmpty(baseAddress, nameof(baseAddress));

            if (masterSessionId == Guid.Empty)
                throw new ArgumentException("masterSessionId");

            _baseAddress = baseAddress;
            _masterSessionId = masterSessionId;
            _timeout = timeout;
        }

        public HttpClient CreateHttpClient()
        {
            var clientHandler = new ApiCookieClientHandler(_baseAddress, _masterSessionId);

            var httpClientHandler = clientHandler.GetHttpClientHandler();
            var httpClient = (new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(_baseAddress),
            });

            httpClient.Timeout = TimeSpan.FromMinutes(_timeout);
            return httpClient;
        }
    }
}