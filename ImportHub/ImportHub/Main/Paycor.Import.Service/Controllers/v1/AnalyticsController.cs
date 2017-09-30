using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Employee;
using Paycor.Import.Extensions;
using Paycor.Import.ImportHistory;
using Swashbuckle.Swagger.Annotations;

namespace Paycor.Import.Service.Controllers.v1
{
    [Authorize]
    [RoutePrefix("importhub/v1")]
    public class AnalyticsController : ApiController
    {
        private readonly ILog _log;
        private readonly IImportHistoryService _importHistoryService;

        public AnalyticsController(ILog log, IImportHistoryService importHistoryService)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            _log = log;
            _importHistoryService = importHistoryService;
        }

        [SwaggerResponse(HttpStatusCode.OK, type: typeof (IEnumerable<ImportAnalysisRow>))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof (ErrorResponse))]
        [HttpGet]
        [Route("analytics")]
        public HttpResponseMessage Get(DateTime? start, DateTime? end)
        {
            _log.Info($"Request received for analytics report from {start} to {end}.");
            try
            {
                var result = _importHistoryService.GetHistoryByDateRange(start, end);
                _log.Info("Analytics request processing completed.");
                return result;
            }
            catch (Exception ex)
            {
                _log.Error("An error occurred while processing an analytics report request.", ex);
                return CreateAndFormatErrorResponse(ex);
            }
        }

        private HttpResponseMessage CreateAndFormatErrorResponse(Exception ex)
        {
            var errorResponse = new ErrorResponse
            {
                CorrelationId = Guid.NewGuid(),
                Detail = ex.Message,
                Title = "An error occurred processing your request...",
                Status = "Error"
            };

            var aggEx = ex as AggregateException;
            if (aggEx != null)
            {
                var count = 0;
                foreach (var innerEx in aggEx.InnerExceptions)
                {
                    errorResponse.Source[$"{count}"] = innerEx.Message;
                    count++;
                }
            }

            var content = JsonConvert.SerializeObject(errorResponse);
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
        }
    }
}
