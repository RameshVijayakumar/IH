using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Adapter;
using Paycor.Import.Http;
using Paycor.Import.Messaging;
using System.Threading.Tasks;

namespace Paycor.Import.Web
{
    public class RestApiExporter : Exporter<RestApiPayload, HttpExporterResult>
    {

        #region Fields
        private readonly ILog _log;
        #endregion

        public RestApiExporter(ILog log)
        {
            _log = log;
        }

        protected override async Task<HttpExporterResult> OnExportAsync(RestApiPayload restApiPayload)
        {

            restApiPayload.TransactionId = (String.IsNullOrEmpty(restApiPayload.TransactionId))
                ? Guid.NewGuid().ToString()
                : restApiPayload.TransactionId;

            HttpExporterResult result = null;
            foreach (var record in restApiPayload.Records)
            {
                var jsonData = JsonConvert.SerializeObject(record);
                result = await PostToApiAsync(jsonData, restApiPayload.ApiEndpoint);
            }

            return result;
        }

        protected virtual async Task<HttpExporterResult> PostToApiAsync(string jsonData, string apiEndpoint)
        {
            HttpExporterResult response;

            try
            {
                _log.DebugFormat("Posting to {0}", apiEndpoint);
                var httpClient = CreateHttpClient();

                httpClient.DefaultRequestHeaders.Accept.Add((new MediaTypeWithQualityHeaderValue("application/json")));

                var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var  responseMessage = await httpClient.PostAsync(apiEndpoint, stringContent);

                response = new HttpExporterResult
                {
                    Response = responseMessage
                };
            }
            catch (Exception e)
            {
                response = new HttpExporterResult
                {
                    Exception = e
                };
            }
            return (response);
        }

        protected virtual HttpClient CreateHttpClient()
        {
            return (new HttpClient(new ApiClientHandler()));
        }
    }
}
