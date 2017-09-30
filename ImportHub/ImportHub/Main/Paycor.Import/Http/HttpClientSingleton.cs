using Paycor.Import.Extensions;
using Paycor.Import.Messaging;
using Paycor.Security.Principal;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Security;
using log4net;

namespace Paycor.Import.Http
{
    public static class HttpClientSingleton
    {
        private static ILog _log;
        private static HttpClientHandler _theHandler = new HttpClientHandler() { UseCookies = false };

        private static HttpClient _instance = null;

        public static void SetLog(ILog log)
        {
            if (log != null)
            {
                _log = log;
            }
        }

        public static HttpClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpClient(_theHandler);
                    _instance.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                return _instance;
            }
        }

        public static HttpRequestMessage CreateHttpRequestMessage(HtmlVerb verb, string requestUri, Guid masterSessionId, 
            string jsonData = null, IDictionary<string, string> headerData = null)
        {
            var request = new HttpRequestMessage(verb.ToMethod(), requestUri);
            request.Headers.TryAddWithoutValidation("Cookie", $"paycorAuth={CreateAuthCookie(masterSessionId).Value}");
            // Allow a context to be passed to the downstream api in the http headers.
            if (headerData != null)
            {
                foreach (var item in headerData)
                {
                    _log?.Debug($"key: {item.Key} , value: {item.Value}");
                    request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }

            if (jsonData != null) request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            return request;
        }

        public static HttpRequestMessage CreateHttpRequestMessage(HtmlVerb verb, string requestUri, string paycorAuth,
            string jsonData = null, IDictionary<string, string> headerData = null)
        {
            var request = new HttpRequestMessage(verb.ToMethod(), requestUri);
            request.Headers.TryAddWithoutValidation("Cookie", $"paycorAuth={paycorAuth}");
            // Allow a context to be passed to the downstream api in the http headers.
            if (headerData != null)
            {
                foreach (var item in headerData)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            if (jsonData != null) request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            return request;
        }


        public static HttpRequestMessage CreateHttpRequestMessage(HtmlVerb verb, 
            string requestUri, string apiKey, string apiSecretKey, 
            string jsonData = null, IDictionary<string, string> headerData = null)
        {
            var request = new HttpRequestMessage(verb.ToMethod(), requestUri);

            AddApiKeyToRequestHeader(request, apiKey, apiSecretKey);
            if (headerData != null)
            {
                foreach (var item in headerData)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            if (jsonData != null) request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            return request;
        }

        private static void AddApiKeyToRequestHeader(HttpRequestMessage request, string apiKey, string apiSecretKey)
        {
            var date = DateTime.UtcNow;
            request.Headers.Date = date;
            var sb = new StringBuilder();
            sb.AppendLine(request.Method.Method);
            if (request.Content != null && request.Content.Headers != null)
            {
                if (request.Content.Headers.ContentMD5 != null)
                {
                    sb.AppendLine(request.Content.Headers.ContentMD5.ToString() ?? string.Empty);
                }
                else
                {
                    sb.AppendLine(string.Empty);
                }
                if (request.Content.Headers.ContentType != null)
                {
                    sb.AppendLine(request.Content.Headers.ContentType.ToString() ?? string.Empty);
                }
                else
                {
                    sb.AppendLine(string.Empty);
                }

            }
            else
            {
                sb.AppendLine(string.Empty);
                sb.AppendLine(string.Empty);
            }
            sb.AppendLine(date.ToString("R"));
            sb.AppendLine(request.RequestUri.PathAndQuery);
            var stringToSign = sb.ToString();

            var signature = GetHmacSignature(apiSecretKey, stringToSign);
            var authstring = $"paycorapi {apiKey}:{signature}";
            request.Headers.TryAddWithoutValidation("Authorization", authstring);
        }

        private static string GetHmacSignature(string privatekey, string message)
        {
            var encoding = new ASCIIEncoding();
            var keyByte = encoding.GetBytes(privatekey);
            var hmacsha1 = new System.Security.Cryptography.HMACSHA1(keyByte);
            var messageBytes = encoding.GetBytes(message);
            var hashmessage = hmacsha1.ComputeHash(messageBytes);

            return Convert.ToBase64String(hashmessage);
        }

        private static HttpCookie CreateAuthCookie(Guid masterSessionId)
        {
            var puf = new PaycorUserFactory();
            var pup = puf.GetPrincipal(masterSessionId);

            var name = FormsAuthentication.FormsCookieName;
            const bool secure = false;
            var domain = FormsAuthentication.CookieDomain;
            var path = FormsAuthentication.FormsCookiePath;

            var cookie = pup.MasterSessionID == Guid.Empty
                ? null
                : PaycorUserFactory.CreateAuthCookie(pup, path, name, domain, secure);

            return cookie;
        }
    }
}
