using System;
using System.Linq;
using System.Net.Http;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.Shared;
using Paycor.Import.Status;

//TODO: No unit tests
namespace Paycor.Import.Http
{
    public class HttpStatusHandler : IHttpStatusHandler
    {
        private readonly ILog _log;
        public HttpStatusHandler(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
        }
        public void AddErrorStatusDetails(MappedFileImportStatusMessage statusMessage, ErrorResponse errorResponse, int rowNumber, HttpExporterResult result)
        {
            statusMessage.Status = MappedFileImportStatusEnum.ImportFileData;

            InitializeRecordCount(statusMessage);
            statusMessage.FailRecordsCount += 1;

            if ((errorResponse?.Source == null && result == null))
            {
                AddStatusDetailsWithErrorReponse(statusMessage, rowNumber, errorResponse);
            }
            else if ((errorResponse?.Source == null) && result.Response != null)
            {
                AddStatusDetailsWithApiResponse(statusMessage, rowNumber, result);
            }
            else if (errorResponse != null)
            {
                AddStatusDetailsWithErrorReponse(statusMessage, rowNumber, errorResponse);
            }
        }

        private static void InitializeRecordCount(ImportStatusMessage statusMessage)
        {
            if (null == statusMessage.FailRecordsCount)
                statusMessage.FailRecordsCount = 0;
            if (null == statusMessage.SuccessRecordsCount)
                statusMessage.SuccessRecordsCount = 0;
        }

        public void SaveStatusFromApi(MappedFileImportStatusMessage statusMessage, MappedFileImportStatusLogger statusLogger, int rowNumber, HttpExporterResult result, ErrorResponse errorResponse = null)
        {
            Ensure.ThatArgumentIsNotNull(statusLogger, nameof(statusLogger));

            InitializeRecordCount(statusMessage);

            if (!result.IsSuccess)
            {
                AddErrorStatusDetails(statusMessage, errorResponse, rowNumber, result);
            }
            else
            {
                var importLink = GetLinkFromApi(result);
                if (importLink != null) statusMessage.ApiLink = importLink;
                statusMessage.SuccessRecordsCount += 1;
            }
            statusLogger.LogMessage(statusMessage);
        }

        public ErrorResponse GetErrorResponse(HttpExporterResult result, MappedFileImportStatusMessage statusMessage)
        {
            try
            {
                var response = result.Response;
                if (response != null)
                {
                    LogHttpResponseErrors(response, statusMessage);
                }

                if (result.Exception != null)
                {
                    if (string.IsNullOrEmpty(statusMessage.Message))
                    {
                        statusMessage.Message = ImportResource.ProcessingException;
                    }
                    _log.Error("Exception details", result.Exception);
                    return new ErrorResponse
                    {
                        Title = ImportResource.ProcessingException
                    };
                }

                var responseContent = result.Response.Content;
                if (null == responseContent)
                {
                    _log.Error(ImportResource.ImportNoResponseContent);
                }

                var resultContent = result.Response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<ErrorResponse>(resultContent);
            }
            catch (Exception exception)
            {
                _log.Error("Unable to Serialize and Read the Error Response", exception);
                return new ErrorResponse()
                {
                    Detail = "An unknown error was returned from the server."
                };
            }
        }
        private void LogHttpResponseErrors(HttpResponseMessage response, MappedFileImportStatusMessage statusMessage)
        {
            var errorMessage = ErrorMessages.HttpResponseErrors(response);
            statusMessage.Message = errorMessage;
            _log.Error(statusMessage.Message);
        }
        private static string GetLinkFromApi(HttpExporterResult result)
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
        private static void AddStatusDetailsWithErrorReponse(MappedFileImportStatusMessage statusMessage, int rowNumber, ErrorResponse errorResponse)
        {
            statusMessage.IssueId = errorResponse.CorrelationId.ToString();
            statusMessage.IssueStatus = errorResponse.Status;
            statusMessage.IssueTitle = errorResponse.Title;
            statusMessage.IssueDetail = errorResponse.Detail;

            if (errorResponse.Source == null || (!errorResponse.Source.Any()))
            {
                AddStatusDetails(statusMessage, rowNumber, "N/A", errorResponse.Detail);
                return;
            }

            foreach (var pair in errorResponse.Source)
            {
                AddStatusDetails(statusMessage, rowNumber, pair.Key, pair.Value);
            }
        }

        private static void AddStatusDetails(MappedFileImportStatusMessage statusMessage, int rowNumber, string columnName, string issue)
        {
            var issueSourceDetails = new MappedFileImportErrorSourceDetail
            {
                RowNumber = rowNumber,
                ColumnName = columnName,
                Issue = issue
            };
            statusMessage.StatusDetails.Add(issueSourceDetails);
        }


        private static void AddStatusDetailsWithApiResponse(MappedFileImportStatusMessage statusMessage, int rowNumber, HttpExporterResult result)
        {
            if (result?.Response != null)
                statusMessage.IssueId = result.Response.StatusCode.ToString();

            if (result?.Response?.ReasonPhrase != null)
                statusMessage.IssueDetail = result.Response.ReasonPhrase.IgnoreSameString(result.Response.StatusCode.ToString());

            AddStatusDetails(statusMessage, rowNumber, "N/A", statusMessage.IssueDetail);
        }
    }
}
