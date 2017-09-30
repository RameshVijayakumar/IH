using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Paycor.Import.Shared;

namespace Paycor.Import.Web
{
    [ExcludeFromCodeCoverage]
    public class WebApiClient : IWebApiClientGet
    {
        public static readonly int DefaultTimeoutInMinutes = 5;

        private readonly HttpClient _httpClient;
        private TimeSpan _timeout = TimeSpan.MinValue;

        #region Properties

        private TimeSpan Timeout
        {
            get
            {
                if (TimeSpan.MinValue == _timeout)
                {
                    var timeout = ConfigurationManager.AppSettings["PerformTimeout"];
                    TimeSpan timeoutValue;

                    _timeout = TimeSpan.TryParse(timeout, out timeoutValue)
                        ? timeoutValue
                        : new TimeSpan(0, DefaultTimeoutInMinutes, 0);
                }
                return (_timeout);
            }
        }

        #endregion

        public WebApiClient()
        {
            _httpClient = new HttpClient(new ApiClientHandler())
            {
                Timeout = Timeout
            };
        }

        public string Get(string uri,
            int retries = 0,
            TimeSpan retryInterval = default(TimeSpan),
            Func<HttpResponseMessage, bool> successEvaluator = null)
        {
            var result = HttpClientHelper(uri, httpClient => httpClient.GetAsync(uri),
                retries,
                retryInterval,
                successEvaluator);
            return result;
        }

        private string HttpClientHelper(string uri,
            Func<HttpClient, Task<HttpResponseMessage>> caller,
            int retries = 0,
            TimeSpan retryInterval = default(TimeSpan),
            Func<HttpResponseMessage, bool> successEvaluator = null)
        {
            string responseContent;
            HttpResponseMessage response;
            bool result;
            try
            {
                var retryProcessor = new ApiHttpClientHelperRetryProcessor(successEvaluator, retries, retryInterval);

                result = retryProcessor.SubmitProcess(Process, caller, out response);

                responseContent = (response.Content != null)
                    ? response.Content.ReadAsStringAsync().Result
                    : "No response provided";

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to reach Api server at : \"{0}\"",
                    uri), ex);
            }

            if (!result || !response.IsSuccessStatusCode)
            {
                throw new Exception(
                    string.Format("PerformAPI returned HTTP response code/description <{0}>, response body: {1}",
                        response.StatusCode, responseContent));
            }

            return responseContent;
        }

        private HttpResponseMessage Process(Func<HttpClient, Task<HttpResponseMessage>> caller)
        {
            HttpResponseMessage response;

            _httpClient.DefaultRequestHeaders.Add("PaycorUseLongToStringConversion", "true");
            try
            {
                response = caller(_httpClient).Result;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occurred communicating with API. Exception: " + ex.Message, ex);
            }

            return response;
        }


        private class ApiHttpClientHelperRetryProcessor :
            RetryProcessor<Func<HttpClient, Task<HttpResponseMessage>>, HttpResponseMessage>
        {
            public ApiHttpClientHelperRetryProcessor(Func<HttpResponseMessage, bool> successEvaluator,
                int retryCount = 0,
                TimeSpan retryInterval = default(TimeSpan))
                : base(successEvaluator ?? SuccessEvaluation, retryCount, retryInterval)
            {
            }

            private static bool SuccessEvaluation(HttpResponseMessage httpResponseMessage)
            {
                return (httpResponseMessage.IsSuccessStatusCode);
            }
        }
    }
}
