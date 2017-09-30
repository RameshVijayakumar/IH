using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using static Paycor.Import.ImportHubTest.Common.Utils;
using System.Threading.Tasks;
using Paycor.Import.ImportHubTest.Common.Authentication;

namespace Paycor.Import.ImportHubTest.Common
{
    public class BaseTestClient
    {
        private HttpClient httpClient;
        private int timeout;
        private bool disposed;
        private string baseUrl;
        private BaseAuthencator authMethod;

        public BaseTestClient()
        {
            //authMethod = new KeypairAuthencator();
        }

        public void Authenticate(BaseAuthencator authenticator)
        {
            authenticator.Authenticate(out httpClient);
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
    }
}
