using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;

//TODO: No unit tests
// ** NOTE **
// These classes should be considered as temporary as it is expected that they will most likely
// be relocated to an external nuget package in the future.
//
namespace Paycor.Import.Extensions
{
    [ExcludeFromCodeCoverage]
    public class ErrorResponse
    {
        public Guid CorrelationId { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public IDictionary<string, string> Source { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public static class ApiControllerExtensions
    {
        private static IHttpActionResult ErrorResponse(this ApiController controller, HttpStatusCode statusCode, ErrorResponse response)
        {
            response.Status = statusCode.ToString();
            return new NegotiatedContentResult<ErrorResponse>(statusCode, response, controller);
        }

        public static IHttpActionResult ValidationResponse(this ApiController controller,
            IDictionary<string, string> errors)
        {
            return HtmlResponse(controller, HttpStatusCode.BadRequest, "Validation Error",
                "A validation error occurred. See the source for more information about specific errors.", errors);
        }

        public static IHttpActionResult HtmlResponse(this ApiController controller, HttpStatusCode statusCode,
            string title, string detail, IDictionary<string, string> errors)
        {
            var errorReponse = new ErrorResponse()
            {
                CorrelationId = Guid.NewGuid(),
                Title = title,
                Detail = detail,
                Source = errors
            };

            return controller.ErrorResponse(statusCode, errorReponse);
        }

        public static IHttpActionResult ExceptionResponse(this ApiController controller, Exception exception, Guid correlationId)
        {
            return ErrorResponse(controller, HttpStatusCode.InternalServerError, new ErrorResponse()
            {
                CorrelationId = correlationId,
                Title = "Error Message Title",
                Detail = exception.Message
            });
        }

        public static IHttpActionResult ExceptionResponse(this ApiController controller, Exception exception)
        {
            return ExceptionResponse(controller, exception, Guid.NewGuid());
        }

        public static HttpResponseMessage ReturnHttpNotFound(this ApiController controller, string message)
        {
            var invalidLookupResult = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(message)
            };
            return invalidLookupResult;
        }

        public static HttpResponseMessage ReturnHttpBadRequest(this ApiController controller, string message)
        {
            var invalidLookupResult = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(message)
            };
            return invalidLookupResult;
        }

        public static HttpResponseMessage ReturnHttpOk(this ApiController controller, byte[] fileBytes, string fileName, string mediaTypeHeaderValue)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileBytes)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaTypeHeaderValue);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return result;
        }
    }
}
 

