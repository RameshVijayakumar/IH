using System;
using static Paycor.Import.ImportHubTest.Common.Utils;
using Paycor.Security.Principal;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Paycor.Import.ImportHubTest.Common
{
    public static class AuthenticationHelper
    {
        public static void AddApiKeyAuthenticationToRequest(HttpRequestMessage httpRequestMessage, DateTime utcDate, string publicKey, string privateKey)
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

        public static async Task<Cookie> GetAuthCookie(HttpRequestMessage httpRequestMessage, DateTime utcDate, string publicKey, string privateKey)
        {
            string domain = httpRequestMessage.RequestUri.GetLeftPart(UriPartial.Authority);
            return await GetAuthCookie(domain, utcDate, publicKey, privateKey);
        }

        public static async Task<Cookie> GetAuthCookie(HttpRequestMessage httpRequestMessage, string username, string password)
        {
            string domain = httpRequestMessage.RequestUri.GetLeftPart(UriPartial.Authority);
            return await GetAuthCookie(domain, username, password);
        }

        public static async Task<Cookie> GetAuthCookie(string domain, DateTime utcDate, string publicKey, string privateKey)
        {
            var handler = new HttpClientHandler {CookieContainer = new CookieContainer()};
            var httpClient = new HttpClient(handler);
            string uri = $"{domain}/accounts/api/session/getavailableclients?codeBaseId=2";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            AddApiKeyAuthenticationToRequest(httpRequestMessage, utcDate, publicKey, privateKey);
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.MediaTypeJson));
            var response = await httpClient.SendAsync(httpRequestMessage);
            var cookie = handler.CookieContainer?.GetCookies(new Uri(domain)).Cast<Cookie>().FirstOrDefault(c => c.Name.Equals("paycorAuth"));
            return cookie;
        }

        public static async Task<Cookie> GetAuthCookie(string domain, string username, string password)
        {
            HttpClientHandler handler = new HttpClientHandler {CookieContainer = new CookieContainer()};
            HttpClient httpClient = new HttpClient(handler);
            var login = HttpUtility.HtmlEncode(username);
            var pass = HttpUtility.HtmlEncode(password);
            string uri = $"{domain}/Accounts/Authentication/DoLogin?ReturnUrl=&UserLogin={login}&Password={pass}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.MediaTypeJson));
            var response = await httpClient.SendAsync(httpRequestMessage);
            var cookie = handler.CookieContainer?.GetCookies(new Uri(domain)).Cast<Cookie>().FirstOrDefault(c => c.Name.Equals("paycorAuth"));
            return cookie;
        }

        public static void GetUserPrincipal( HttpClient client)
        {
            var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            var httpClient = new HttpClient(handler);
            var principal = HttpContext.Current.User as PaycorUserPrincipal;
            var factory = new PaycorUserFactory();
            var cookie = PaycorUserFactory.CreateAuthCookie(principal);
           
        }
    }
}
