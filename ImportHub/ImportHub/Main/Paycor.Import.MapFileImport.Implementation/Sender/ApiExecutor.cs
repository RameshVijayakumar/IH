using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation.Sender
{
    public class ApiExecutor : IApiExecutor
    {
        private readonly ILog _logger;
        private readonly IHttpInvoker _httpInvoker;
        private readonly IGenerateFailedRecord _generateFailedRecord;
        public ApiExecutor(ILog logger, IHttpInvoker httpInvoker, IGenerateFailedRecord generateFailedRecord)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(httpInvoker, nameof(httpInvoker));
            Ensure.ThatArgumentIsNotNull(generateFailedRecord, nameof(generateFailedRecord));
            _logger = logger;
            _httpInvoker = httpInvoker;
            _generateFailedRecord = generateFailedRecord;
        }

        public async Task<ApiResult> ExecuteApiAsync(Guid masterSessionId, PayloadData payloadDataItem,
            IDictionary<string, string> headerData)
        {
            var apiResult = new ApiResult();
            try
            {
                _logger.Info(
                    $"Calling endpoint: {payloadDataItem.EndPoint} with Verb: {payloadDataItem.HtmlVerb}");

                var result = await _httpInvoker.CallApiEndpointAsync(masterSessionId, payloadDataItem.PayLoad,
                    payloadDataItem.EndPoint,
                    payloadDataItem.HtmlVerb, headerData);

                apiResult.ApiLink = result.GetLinkFromApi();

                _logger.Debug(payloadDataItem.PayLoad);

                if (result.IsSuccess)
                {
                    _logger.Info($"Success result received from endpoint: {payloadDataItem.EndPoint} and Verb: {payloadDataItem.HtmlVerb} with Code: {result.Response?.StatusCode}");
                    return apiResult;
                }

                var errorResponse = result.GetErrorResponse(_logger);

                if (payloadDataItem.ApiRecord != null)
                {
                    apiResult.ErrorResultDataItem = new ErrorResultData
                    {
                        ErrorResponse = errorResponse,
                        FailedRecord = _generateFailedRecord.GetFailedRecord(payloadDataItem.ApiRecord, errorResponse,
                            result),
                        HttpExporterResult = result,
                        RowNumber = payloadDataItem.ApiRecord.RowNumber,
                        ImportType = payloadDataItem.ApiRecord.ImportType
                    };
                }
                _logger.Warn($"Failure result received from endpoint: {payloadDataItem.EndPoint} and Verb: {payloadDataItem.HtmlVerb} with Code: {result.Response?.StatusCode} and error response: Title: {errorResponse.Title}, Correlation Id: {errorResponse.CorrelationId}, Detail: {errorResponse.Detail}.");
                return apiResult;
            }
            catch (Exception exception)
            {
                _logger.Error(
                    $"An Error Occurred in {nameof(ApiExecutor)}:{nameof(ExecuteApiAsync)} ",
                    exception);

                if (payloadDataItem?.ApiRecord != null)
                {
                    apiResult.ErrorResultDataItem = new ErrorResultData
                    {
                        ErrorResponse = new ErrorResponse { Detail = exception.ToString() },
                        FailedRecord = _generateFailedRecord.GetFailedRecord(payloadDataItem.ApiRecord,
                            new ErrorResponse { Detail = exception.ToString() }, null),
                        HttpExporterResult = null,
                        RowNumber = payloadDataItem.ApiRecord.RowNumber,
                        ImportType = payloadDataItem.ApiRecord.ImportType
                    };
                }
                return apiResult;
            }
        }
    }
}