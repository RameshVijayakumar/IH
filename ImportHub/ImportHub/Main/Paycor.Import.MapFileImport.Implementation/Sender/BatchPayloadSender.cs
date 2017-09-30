using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation.Sender
{
    public class BatchPayloadSender : IPayloadSender
    {
        private readonly ILog _logger;
        private readonly IHttpInvoker _httpInvoker;
        private readonly IGenerateFailedRecord _generateFailedRecord;
        private List<ErrorResultData> _errorResultDataItems;
        public BatchPayloadSender(ILog logger, IHttpInvoker httpInvoker, IGenerateFailedRecord generateFailedRecord)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(httpInvoker, nameof(httpInvoker));
            Ensure.ThatArgumentIsNotNull(generateFailedRecord, nameof(generateFailedRecord));

            _logger = logger;
            _httpInvoker = httpInvoker;
            _generateFailedRecord = generateFailedRecord;
        }

        public bool GetPayloadSenderType()
        {
            return true;
        }

        public async Task<PayloadSenderResponse> SendAsync(ImportContext context, IEnumerable<PayloadData> payloadDataItems)
        {
            var apiLinks = new List<string>();
            var importType = string.Empty;
            try
            {
                _errorResultDataItems = new List<ErrorResultData>();
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                if (payloadDataItems == null)
                    throw new ArgumentNullException(nameof(payloadDataItems));

                var payloadDatas = payloadDataItems as PayloadData[] ?? payloadDataItems.ToArray();

                importType = payloadDatas.FirstOrDefault()?.ApiRecord?.ImportType;
                _logger.Debug($"{nameof(PayloadSender)} entered.");

                var headerData = new Dictionary<string, string>
                {
                    // All header elements should start with "X-".
                    ["X-Import-TransactionId"] = context.TransactionId
                };

                var payloadSenderResponse = new PayloadSenderResponse();

                var payloadDataItemsList = payloadDataItems as IList<PayloadData> ?? payloadDatas.ToList();
                var statusResult = await CallApiForBatchAsync(Guid.Parse(context.MasterSessionId), payloadDataItemsList, headerData, apiLinks);
                payloadSenderResponse.Status = statusResult ? Status.Success : Status.Failure;
                payloadSenderResponse.ApiLinks = apiLinks;

                var link = apiLinks.FirstOrDefault(x => !string.IsNullOrEmpty(x));
                if (link != null)
                {
                    _logger.Debug($"ApiLink received from response: {link}.");
                }
                payloadSenderResponse.ErrorResultDataItems = _errorResultDataItems;

                if (payloadDataItemsList.Count == _errorResultDataItems.Count)
                {
                    _logger.Debug("All the records in the send payload failed");
                }
                return payloadSenderResponse;
            }
            catch (Exception exception)
            {
                _logger.Fatal($"An Error Occurred in {nameof(PayloadSender)}:{nameof(SendAsync)} ", exception);
                _logger.Debug($"Exception occurred in the sender for chunk: {context?.ChunkNumber}");

                var errorResultData = new ErrorResultData
                {
                    ErrorResponse =
                        new ErrorResponse
                        {
                            Detail = "A problem occurred during the import process, please contact Paycor Specialist"
                        },
                    FailedRecord = null,
                    HttpExporterResult = null,
                    RowNumber = 0,
                    ImportType = importType
                };
                if (_errorResultDataItems.Count == 0)
                    _errorResultDataItems.Add(errorResultData);

                return new PayloadSenderResponse
                {
                    Status = Status.Failure,
                    Error = exception,
                    ApiLinks = apiLinks,
                    ErrorResultDataItems = _errorResultDataItems
                };
            }
        }

        private async Task<bool> CallApiForBatchAsync(Guid masterSessionId, IEnumerable<PayloadData> payloadDataItems,
           IDictionary<string, string> headerData, ICollection<string> apiLinks)
        {
            BatchImportResponse batchImportResult;
            var payloadData = payloadDataItems.FirstOrDefault();

            _logger.Debug($"calling endpoint: {payloadData.EndPoint} with verb: {payloadData.HtmlVerb} and payload");
            
            var result = await _httpInvoker.CallApiEndpointAsync(masterSessionId, payloadData.PayLoad,
                payloadData.EndPoint,
                payloadData.HtmlVerb, headerData);
                        
            apiLinks.Add(result.GetLinkFromApi());
            try
            {
                batchImportResult =
                    JsonConvert.DeserializeObject<BatchImportResponse>(
                        result.Response?.Content?.ReadAsStringAsync().Result);

                if (batchImportResult?.ErrorResponseDataBatch == null)
                    return GetStatusForNonResponseBatch(payloadData, result);
            }
            catch (JsonSerializationException)
            {
                return GetStatusForNonResponseBatch(payloadData, result);
            }
            catch (Exception)
            {
                _logger.Debug($"Exception thrown while parsing the {nameof(BatchImportResponse)}");
                return GetStatusForNonResponseBatch(payloadData, result);
            }
            foreach (var errorResponseData in batchImportResult.ErrorResponseDataBatch)
            {
                var failedApiRecord = payloadData.ApiRecords.GetApiRecordByRowNumber(errorResponseData.BatchLine);
                var errorResponse = new ErrorResponse
                {
                    CorrelationId = batchImportResult.CorrelationId,
                    Status = errorResponseData.Status,
                    Title = errorResponseData.ErrorResponseBatchRowEntry?.Title,
                    Detail = errorResponseData.ErrorResponseBatchRowEntry?.Detail,
                    Source = errorResponseData.ErrorResponseBatchRowEntry?.Source
                };
                var errorResultData = new ErrorResultData
                {
                    RowNumber = failedApiRecord.RowNumber,
                    ErrorResponse = errorResponse,
                    FailedRecord = _generateFailedRecord.GetFailedRecord(failedApiRecord, errorResponse, result),
                    ImportType = failedApiRecord.ImportType
                };
                _errorResultDataItems.Add(errorResultData);
            }
            return result.IsSuccess;
        }

        private bool GetStatusForNonResponseBatch(PayloadData payloadData, HttpExporterResult result)
        {   
            if (result.IsSuccess)
                _logger.Debug($"Batch records in the send payload success with status code {result.Response?.StatusCode}");
            else
            {
                _logger.Error("Batch records in the send payload failed");
                _logger.Debug(
                    result.Response == null
                        ? "No response is not present in the result."
                        : $"Response status code: {result.Response?.StatusCode} with content: {result.GetResponseContent()}");
                _errorResultDataItems.Add(new ErrorResultData
                {
                    ErrorResponse = new ErrorResponse
                    {
                        Detail = result.Response == null ? "A problem occurred during the import process, please contact Paycor Specialist" : result.Response.ReasonPhrase
                    },
                    ImportType = payloadData.ApiRecords?.FirstOrDefault()?.ImportType,
                    RowNumber = 0  
                });
            }
            return result.IsSuccess;
        }
    }
}
