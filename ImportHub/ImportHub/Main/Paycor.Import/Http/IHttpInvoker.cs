using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Paycor.Import.Messaging;

namespace Paycor.Import.Http
{
    public interface IHttpInvoker
    {
        HttpExporterResult CallApiEndpoint(Guid masterSessionId, string jsonData, string apiEndpoint, HtmlVerb verb, IDictionary<string, string> headerData = null);
        Task<HttpExporterResult> CallApiEndpointAsync(Guid masterSessionId, string jsonData, string apiEndpoint, HtmlVerb verb, IDictionary<string, string> headerData = null);

        Task<HttpExporterResult> CallApiEndpointWithApiKeyAsync(string apiKey,
            string apiSecretKey, 
            string jsonData, string apiEndpoint, HtmlVerb verb, 
            IDictionary<string, string> headerData = null);

        Task<HttpExporterResult> CallApiEndpointWithApiKeyAsync(string paycorAuth,
            string jsonData, string apiEndpoint, HtmlVerb verb,
            IDictionary<string, string> headerData = null);
    }
}