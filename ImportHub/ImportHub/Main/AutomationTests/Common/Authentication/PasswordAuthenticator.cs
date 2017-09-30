using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Paycor.Import.ImportHubTest.Common.Authentication
{
    public class PasswordAuthenticator : BaseAuthencator
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public PasswordAuthenticator(string userName, string password, string domain)
        {
            BaseUrl = domain;
            UserName = userName;
            Password = password;
        }

        public override void Authenticate(out HttpClient client) 
        {
            GetCookie();
            var cookies = new CookieContainer();
            cookies.Add(Cookie);
            client = new HttpClient(new HttpClientHandler() {CookieContainer = cookies});
        }

        private void GetCookie()
        {
            using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer()})
            {
                string url;
                if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password))
                {
                    url = $"{BaseUrl}{Constants.ApiAutoPath}";
                }
                else
                { 
                    handler.Credentials = new NetworkCredential(UserName, Password);
                    url = $"{BaseUrl}{Constants.ApiLoginPath}";
                }
                using (var ckClient = new HttpClient(handler))
                {
                    ckClient.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue(Constants.MediaTypeJson));
                    ckClient.GetAsync(url).Result.EnsureSuccessStatusCode();
                }
                Cookie = handler.CookieContainer?.GetCookies(new Uri(BaseUrl))
                    .Cast<Cookie>().FirstOrDefault(c => c.Name.Equals("paycorAuth"));
                Assert.IsNotNull(Cookie, $" *** Failed to get auth cookie using UserName = { UserName}, Password = '#####' ");
                Utils.Log($"Success: Get auth cookie using UserName = { UserName}, Password = '#####' ");
            }
        }
    }
}
