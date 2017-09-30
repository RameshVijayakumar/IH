using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AsyncStackTrace;
using static Paycor.Import.ImportHubTest.Common.Utils;

namespace Paycor.Import.ImportHubTest.Common
{
    public class TestClientBase : IDisposable
    {
        HttpClient _httpClient;
        double _timeout;
        bool _disposed = false;
        public string BaseUrl { get; }

        public TestClientBase()
        {
            BaseUrl = BaseUrl ?? ConfigurationManager.AppSettings["BaseUrl"];
            Double.TryParse(ConfigurationManager.AppSettings["HttpClient.Timeout"], out _timeout);
            _timeout = _timeout == 0d  ? Constants.Timeout : _timeout;
            CreateClientUseApiKey(ConfigurationManager.AppSettings["PrivateKey"], ConfigurationManager.AppSettings["PublicKey"]);
        }

        public void CreateClientUseLoginPassword(string username, string password)
        {
            Log($"Try get authentication using username={username}, password={MaskString(password, 2)}, for domainUrl={BaseUrl}");

            var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
             var cookie = AuthenticationHelper.GetAuthCookie(BaseUrl, username, password).Result;
            if (cookie == null)
                throw new AutomationTestException($"Failed to get Authentication for{username}/{MaskString(password, 2)} at {BaseUrl}");
            handler.CookieContainer.Add(cookie);
            _httpClient?.Dispose();
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(_timeout) };
            Log("Client authentication succeed.");
        }

        public void CreateCliantUseHmacOnly(HttpRequestMessage httpRequestMessage, string privateKey, string publicKey)
        {
            Log($"Try get authentication using privateKey={MaskString(privateKey, 4)}, publicKey={MaskString(publicKey, 4)}, for baseUrl={BaseUrl}");
            var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            Log(@"Try use HMAC Signing.");
            AuthenticationHelper.AddApiKeyAuthenticationToRequest(httpRequestMessage, DateTime.UtcNow, publicKey, privateKey);
            _httpClient?.Dispose();
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(_timeout) };
            Log("Client authentication succeed.");
        }

        public void CreateClientUseApiKey(string privateKey, string publicKey)
        {
            Log($"Try get authentication using privateKey={MaskString(privateKey, 4)}, publicKey={MaskString(publicKey, 4)}, for baseUrl={BaseUrl}");
            var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            Log(@"Try use api endpoint to authenticate");
            var cookie = AuthenticationHelper.GetAuthCookie(BaseUrl, DateTime.UtcNow, publicKey, privateKey).Result;
            if (cookie == null)
                throw new AutomationTestException($"Failed to get Authentication for privatekey={MaskString(privateKey, 4)}, publickey={MaskString(publicKey, 4)} at {BaseUrl}");
            handler.CookieContainer.Add(cookie);
            _httpClient?.Dispose();
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(_timeout) };
            Log("Client authentication succeed.");
        }

        public async Task<HttpResponseMessage> TrySendAsync(HttpRequestMessage httpRequestMessage)
        {
            try
            {
                return await _httpClient.SendAsync(httpRequestMessage).Trace();
            }
            catch (Exception e)
            {
                Log(e.Message);
                throw ;
            }
        }

        public async Task<HttpResponseMessage> CallGet(string query, string mediaType = Constants.MediaTypeJson)
        {
            return await TrySendAsync(GenerateRequestMessage(HttpMethod.Get, query, null, mediaType)).Trace();
        }

        public async Task<HttpResponseMessage> CallDelete(string query, string payload = null)
        {
            return await TrySendAsync(GenerateRequestMessage(HttpMethod.Delete, query, payload)).Trace();
        }

        public async Task<HttpResponseMessage> CallPatch(string query, string payload, string mediaType = Constants.MediaTypeJson)
        {
            return await TrySendAsync(GenerateRequestMessage(new HttpMethod("PATCH"), query, payload, mediaType)).Trace();
        }

        public async Task<HttpResponseMessage> CallPut(string query, string payload, string mediaType = Constants.MediaTypeJson)
        {
            return await TrySendAsync(GenerateRequestMessage(HttpMethod.Put, query, payload)).Trace();
        }

        public async Task<HttpResponseMessage> CallPost(string query, string payload, string mediaType = Constants.MediaTypeJson)
        {
            return await TrySendAsync(GenerateRequestMessage(HttpMethod.Post, query, payload)).Trace();
        }

        public async Task<HttpResponseMessage> CallPost(string query, HttpContent content, string mediaType = Constants.MediaTypeJson)
        {
            return await TrySendAsync(GenerateRequestMessage(HttpMethod.Post, GetAbsoluteUri(query), content, mediaType)).Trace();
        }

        public async Task<HttpResponseMessage> CallTrace(string query, string payload)
        {
            return await TrySendAsync(GenerateRequestMessage(HttpMethod.Trace, query, payload));
        }

        public IEnumerable<HttpResponseMessage> CallWaitUntil(HttpRequestMessage httpRequestMessage, HttpStatusCode expectedStatusCode, uint sleepSeconds = 15)
        {
            HttpStatusCode code = (HttpStatusCode) 0;
            while (!code.Equals(expectedStatusCode))
            {
                var message = TrySendAsync(httpRequestMessage).Result;
                code = message.StatusCode;
                Thread.Sleep(TimeSpan.FromSeconds(sleepSeconds));
                yield return message;
            }
        }

        public async Task<HttpResponseMessage> CallWithRetry(HttpRequestMessage httpRequestMessage, uint retry = 0)
        {
            for(var i = 0; i < retry; i++)
            {
                var message = await TrySendAsync(httpRequestMessage);
                if (message != null)
                    return message;
                await Task.Delay(TimeSpan.FromSeconds(i * 10));
            }
            return null;
        }

        public HttpRequestMessage GenerateRequestMessage(HttpMethod method, string query, string payload = null , string mediaType = Constants.MediaTypeJson)
        {
            HttpContent content = null;
            if (!string.IsNullOrEmpty(payload))
            {
                content = new StringContent(payload, Encoding.UTF8, mediaType);
                Log($"Contnet payload= {content.ReadAsStringAsync().Result.Serialize()}");
            }
            
            return GenerateRequestMessage(method, GetAbsoluteUri(query), content, mediaType);
        }

        public HttpRequestMessage GenerateRequestMessage(HttpMethod method, Uri requestUri, HttpContent content, string mediaType)
        {
            Log("Create httpRequestMessage: ");
            Verb vb;
            if (!Enum.TryParse(method.Method, true, out vb))
            {
                throw new AutomationTestException($"Error: Unsupported http method detected= {method.Method}");
            }
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = requestUri
            };
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            if (content != null)
            {
                httpRequestMessage.Content = content;
            }
            else if (method == HttpMethod.Put || method == HttpMethod.Post)
            {
                throw new AutomationTestException($"Error: Content is missing for {method}");
            }
            Log($"\tMethod= {httpRequestMessage?.Method}");
            Log($"\tRequestUri= {httpRequestMessage?.RequestUri}");
            Log(httpRequestMessage.Headers.Serialize());
            Log(httpRequestMessage.Content.Serialize());
            return httpRequestMessage;
        }

        public void CreateDefaultClient()
        {
            _httpClient?.Dispose();
            var handler = new HttpClientHandler { UseDefaultCredentials = true };
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(_timeout) };
            AuthenticationHelper.GetUserPrincipal(_httpClient);
        }

        private Uri GetAbsoluteUri(string query)
        {
            Uri uri;
            if (Uri.TryCreate(query, UriKind.Absolute, out uri))
            {
                string targetUri = uri.GetLeftPart(UriPartial.Authority);
                if (!targetUri.Equals(BaseUrl))
                {
                    Log($"target query contains different domainUri {targetUri}, default: {BaseUrl}");
                }
            }
            else
            {
                Uri.TryCreate(new Uri(BaseUrl), query, out uri);
            }
            return uri;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
