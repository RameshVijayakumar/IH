using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure;

namespace Paycor.Import.Web
{
    public class ApiClientHandler : HttpClientHandler
    {
        private string _apiKey;
        private string _apiSecretKey;

        public ApiClientHandler()
        {
            _apiKey = CloudConfigurationManager.GetSetting("PerformSecurityKey");
            _apiSecretKey = CloudConfigurationManager.GetSetting("PerformSecuritySecretKey");
        }

        protected override Task<HttpResponseMessage>
            SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var date = DateTime.UtcNow;
            request.Headers.Date = date;

            var signature = CreateSignature(request, date, _apiSecretKey);
            
            var authstring = string.Format("paycorapi {0}:{1}", _apiKey, signature);
            request.Headers.TryAddWithoutValidation("Authorization", authstring);

            return base.SendAsync(request, cancellationToken);
        }

        public static string CreateSignature(HttpRequestMessage request, DateTime date, string apiSecretKey)
        {
            Ensure.ThatArgumentIsNotNull(request, nameof(request));

            if (string.IsNullOrEmpty(apiSecretKey))
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine(request.Method.Method);

            if (request.Content != null && request.Content.Headers != null)
            {
                if (request.Content.Headers.ContentMD5 != null)
                {
                    sb.AppendLine(request.Content.Headers.ContentMD5.ToString());
                }
                else
                {
                    sb.AppendLine(string.Empty);
                }
                if (request.Content.Headers.ContentType != null)
                {
                    sb.AppendLine(request.Content.Headers.ContentType.ToString());
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
            return signature;
        }

        private static string GetHmacSignature(string privatekey, string message)
        {
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(privatekey);

            var hmacsha1 = new System.Security.Cryptography.HMACSHA1(keyByte);
            byte[] messageBytes = encoding.GetBytes(message);
            byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }
    }
}
