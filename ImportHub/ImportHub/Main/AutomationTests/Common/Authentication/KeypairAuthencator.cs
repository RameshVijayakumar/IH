using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Paycor.Import.ImportHubTest.Common.Authentication
{
    public class KeypairAuthencator : BaseAuthencator
    {
        protected string PrivateKey { get; set; }
        protected string PublicKey { get; set; }

        public KeypairAuthencator(string domain, string publicKey, string privateKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
            BaseUrl = domain;
        }

        public override void Authenticate(out HttpClient client)
        {
            GetAuthCookie();
            var cookies = new CookieContainer();
            cookies.Add(Cookie);
            client = new HttpClient(new HttpClientHandler() { CookieContainer = cookies });
        }

        private void GetAuthCookie()
        {
            var handler = new WebRequestHandler() {CookieContainer = new CookieContainer()};
            using (var ckClient = new HttpClient(handler))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{Constants.ApiKeyPath}");
                AddApiKeyAuthenticationToRequest(request, DateTime.UtcNow, PublicKey, PrivateKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.MediaTypeJson));
                ckClient.SendAsync(request).Result.EnsureSuccessStatusCode();
                Cookie = handler.CookieContainer?.GetCookies(
                    new Uri($"{BaseUrl}")).Cast<Cookie>().FirstOrDefault(c => c.Name.Equals("paycorAuth"));
                Assert.IsNotNull(Cookie,
                    $" *** Failed to get auth cookie. publicKey= {PublicKey.Substring(0, 3)}####, privateKey= {PrivateKey.Substring(0, 3)}####");
                Utils.Log(
                    $"Success: Get auth cookie. publicKey= {PublicKey.Substring(0, 3)}####, privateKey= {PrivateKey.Substring(0, 3)}####");
            }
        }

        private static void AddApiKeyAuthenticationToRequest(HttpRequestMessage httpRequestMessage, DateTime utcDate, string publicKey, string privateKey)
        {
            var sb = new StringBuilder();
            sb.AppendLine(httpRequestMessage.Method.Method);
            sb.AppendLine(httpRequestMessage.Content?.Headers.ContentMD5?.ToString() ?? string.Empty);
            sb.AppendLine(httpRequestMessage.Content?.Headers.ContentType?.ToString() ?? string.Empty);
            sb.AppendLine(utcDate.ToString("R"));
            sb.AppendLine(httpRequestMessage.RequestUri.PathAndQuery);
            var encoding = new ASCIIEncoding();
            var keyByte = encoding.GetBytes(privateKey);
            var messageBytes = encoding.GetBytes(sb.ToString());
            var hmacsha1 = new HMACSHA1(keyByte);
            var hashmessage = hmacsha1.ComputeHash(messageBytes);
            var signature = Convert.ToBase64String(hashmessage);
            var authstring = $"paycorapi {publicKey}:{signature}";
            httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", authstring);
            httpRequestMessage.Headers.Date = utcDate;
        }
    }
}
