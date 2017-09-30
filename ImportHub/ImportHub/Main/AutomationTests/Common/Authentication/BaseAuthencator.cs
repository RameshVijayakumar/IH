using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Paycor.Import.ImportHubTest.Common.Authentication
{
    public abstract class BaseAuthencator
    {
        protected  OAuthToken OAuthTokey { get; set; }
        protected Cookie Cookie { get; set; }
        protected string BaseUrl { get; set; }
        public abstract void Authenticate(out HttpClient client);
    }

    public class OAuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("mfaProcess")]
        public string MfaProcess { get; set; }
        [JsonProperty("issued")]
        public string Issued { get; set; }
        [JsonProperty("expires")]
        public string Expires { get; set; }
    }
}
