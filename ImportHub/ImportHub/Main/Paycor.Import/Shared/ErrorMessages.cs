using System.Net;
using System.Net.Http;
//TODO: No unit tests

namespace Paycor.Import.Shared
{
    public static class ErrorMessages
    {
        public static string HttpResponseErrors(HttpResponseMessage response)
        {
            string errorMessage;
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    errorMessage = ImportResource.ImportServiceAuthenticationFailure;
                    break;
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.BadGateway:
                    errorMessage = ImportResource.ImportServiceUnresponsive;
                    break;
                case HttpStatusCode.Gone:
                    errorMessage = ImportResource.ResourceAlreadyRemoved;
                    break;
                case HttpStatusCode.NotFound:
                    errorMessage = ImportResource.ResourceNotFound;
                    break;
                case HttpStatusCode.InternalServerError:
                    errorMessage = ImportResource.ImportServiceResponseUnexpected;
                    break;
                default:
                    errorMessage = string.Format(ImportResource.ImportServiceResponseUnexpectedWithMsg,
                        response.StatusCode, response.ReasonPhrase);
                    break;
            }
            return errorMessage;
        }
    }
}