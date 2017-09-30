using System;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Http;
using Paycor.Import.Shared;

//TODO: No unit tests
namespace Paycor.Import.Extensions
{
    public static class HttpExporterResultExtensions
    {
        public static string GetLinkFromApi(this HttpExporterResult result)
        {
            try
            {
                return
                    result?.Response?.Headers?.GetValues("Link")?
                        .SingleOrDefault(x => x.ToLower().Replace("\"", "").Replace("'", "").Contains("rel=import"));
            }
            catch (InvalidOperationException)
            {
                // Eat the possible exception that gets thrown if "Link" is not found in the response header.
                return null;
            }
        }

        public static ErrorResponse GetErrorResponse(this HttpExporterResult result, ILog logger)
        {
            try
            {
                Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
                var response = result.Response;
                if (response != null)
                {
                    logger.Debug(ErrorMessages.HttpResponseErrors(response));
                }

                if (result.Exception != null)
                {
                    logger.Error("Exception details", result.Exception);
                    return new ErrorResponse
                    {
                        Title = ImportResource.ProcessingException
                    };
                }

                var resultContent = result.Response.Content.ReadAsStringAsync().Result;                            
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(resultContent);
                    if (string.Compare(errorResponse.Title, "InternalServerError", StringComparison.OrdinalIgnoreCase) ==
                         0)
                    {
                        errorResponse.Title = "A problem occurred";
                        errorResponse.Status = "Another problem occurred";
                        errorResponse.Detail =
                            "A problem occurred during the import process. Please contact your Paycor Client Specialist for assistance.";
                    }
                    return errorResponse;
                }
                catch (Exception)
                {
                    // If there is no content, or it isn't of the right type, then just send back an
                    // custom ErrorResponse.
                    logger.Warn($"#UnexpectedResponse An API was called, but it did not return the expected response {result.Response?.StatusCode}. Please contact the API owners and have them correct this issue.");
                    logger.Info($"Response content is {resultContent}");
                    return new ErrorResponse
                    {
                        Title = "A problem occurred",
                        Detail = "A problem occurred during the import process. Please contact your Paycor Client Specialist for assistance."
                    };
                }
            }
            catch (Exception exception)
            {
                logger.Warn("#UnexpectedResponse Unable to Serialize and Read the Error Response", exception);
                return new ErrorResponse
                {
                    Title = "A problem occurred",
                    Detail = "A problem occurred during the import process. Please contact your Paycor Client Specialist for assistance."
                };
            }
        }

        public static string GetResponseContent(this HttpExporterResult result)
        {
            const string noResponse = "No response provided";

            return result?.Response?.Content == null ? noResponse : result?.Response?.Content?.ReadAsStringAsync().Result;
        }
    }
}
