using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Messaging;
//TODO: No unit tests
namespace Paycor.Import.Http
{
    public class HttpInvoker : IHttpInvoker
    {
        private readonly ILog _log;

        public HttpInvoker(ILog log)
        {
            _log = log;
            HttpClientSingleton.SetLog(_log);
        }

        public HttpExporterResult CallApiEndpoint(Guid masterSessionId, string jsonData, string apiEndpoint, HtmlVerb verb, IDictionary<string, string> headerData = null)
        {
            try
            {
                var result = CallApiEndpointAsync(masterSessionId, jsonData, apiEndpoint, verb, headerData).Result;
                return result;
            }
            catch (Exception ex)
            {
                var httpExporterResult = new HttpExporterResult
                {
                    Exception = ex
                };
                return httpExporterResult;
            }
        }

        public async Task<HttpExporterResult> CallApiEndpointAsync(Guid masterSessionId, string jsonData, string apiEndpoint, HtmlVerb verb, IDictionary<string, string> headerData = null)
        {
            try
            {
                var request = HttpClientSingleton.CreateHttpRequestMessage(verb, apiEndpoint, masterSessionId, jsonData, headerData);

                var result = await HttpClientSingleton.Instance.SendAsync(request);

                _log.Debug($"{verb} to {apiEndpoint}");
                return new HttpExporterResult
                {
                    Response = result
                };
            }
            catch (Exception ex)
            {
                _log.Error($"Error occurred while calling the API {apiEndpoint} at {nameof(HttpInvoker)}", ex);
                var httpExporterResult = new HttpExporterResult
                {
                    Exception = ex
                };
                return httpExporterResult;
            }
        }
        public async Task<HttpExporterResult> CallApiEndpointWithApiKeyAsync(string paycorAuth, string jsonData, string apiEndpoint, HtmlVerb verb,
            IDictionary<string, string> headerData = null)
        {
            try
            {
                var request = HttpClientSingleton.CreateHttpRequestMessage(verb, apiEndpoint, paycorAuth, jsonData, headerData);

                var result = await HttpClientSingleton.Instance.SendAsync(request);

                _log.Debug($"{verb} to {apiEndpoint}");
                return new HttpExporterResult
                {
                    Response = result
                };
            }
            catch (Exception ex)
            {
                _log.Error($"Error occurred while calling the API {apiEndpoint} at {nameof(HttpInvoker)}", ex);
                var httpExporterResult = new HttpExporterResult
                {
                    Exception = ex
                };
                return httpExporterResult;
            }
        }
 
        public async Task<HttpExporterResult> CallApiEndpointWithApiKeyAsync(string apiKey, string apiSecretKey, string jsonData, string apiEndpoint,
            HtmlVerb verb, IDictionary<string, string> headerData = null)
        {
            try
            {
                var request = HttpClientSingleton.CreateHttpRequestMessage(verb, apiEndpoint, apiKey, apiSecretKey, jsonData, headerData);
                var result = await HttpClientSingleton.Instance.SendAsync(request);


                _log.Debug($"{verb} to {apiEndpoint}");
                return new HttpExporterResult
                {
                    Response = result
                };
            }
            catch (Exception ex)
            {
                _log.Error($"Error occurred while calling the API {apiEndpoint} at {nameof(HttpInvoker)}", ex);
                var httpExporterResult = new HttpExporterResult
                {
                    Exception = ex
                };
                return httpExporterResult;
            }
        }


    }
}
