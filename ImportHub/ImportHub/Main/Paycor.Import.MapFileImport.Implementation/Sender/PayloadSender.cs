using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.MapFileImport.Implementation.Sender
{
    public class PayloadSender : IPayloadSender
    {
        private readonly ILog _logger;
        private List<ErrorResultData> _errorResultDataItems;
        private readonly IApiExecutor _apiExecutor;

        public PayloadSender(ILog logger, IApiExecutor apiExecutor)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(apiExecutor, nameof(apiExecutor));

            _logger = logger;
            _apiExecutor = apiExecutor;
        }

        public bool GetPayloadSenderType()
        {
            return false;
        }

        public async Task<PayloadSenderResponse> SendAsync(ImportContext context,
            IEnumerable<PayloadData> payloadDataItems)
        {
            _errorResultDataItems = new List<ErrorResultData>();
            var importType = string.Empty;
            var apiLinks = new List<string>();
            try
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                if (payloadDataItems == null)
                    throw new ArgumentNullException(nameof(payloadDataItems));

                _logger.Debug($"{nameof(SendAsync)} entered.");

                var headerData = GetRequestHeader(context);

                var payloadSenderResponse = new PayloadSenderResponse();
                var payloadDataItemsList = payloadDataItems.ToList();
                importType = payloadDataItemsList.FirstOrDefault()?.ApiRecord?.ImportType;

                var apiResults = await CallApiAsync(context, payloadDataItems, headerData);
                _errorResultDataItems = apiResults.Where(t => t.ErrorResultDataItem != null)
                    .Select(t => t.ErrorResultDataItem).ToList();
                payloadSenderResponse.ErrorResultDataItems = _errorResultDataItems;
                payloadSenderResponse.ApiLinks = apiResults.Select(t => t.ApiLink).ToList();
                apiLinks = payloadSenderResponse.ApiLinks.ToList();

                var link = payloadSenderResponse.ApiLinks.FirstOrDefault(x => !string.IsNullOrEmpty(x));
                if (link != null)
                {
                    _logger.Debug($"ApiLink received from response: {link}.");
                }

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

        private static Dictionary<string, string> GetRequestHeader(ImportContext context)
        {
            if (!context.DelayProcess)
            {
                return new Dictionary<string, string>
                {
                    ["X-Import-TransactionId"] = context.TransactionId
                };
            }
            if (context.ImportHeaderInfo == null)
            {
                return new Dictionary<string, string>
                {
                    ["X-Import-TransactionId"] = context.TransactionId,
                    ["X-Import-TotalRecordCount"] = context.TotalRecordCount.ToString(),
                    ["X-Import-TotalTabs"] = context.TotalTabs.ToString()
                };
            }
            var headerData = new Dictionary<string, string>
            {
                ["TransactionId"] = context.TransactionId,
                ["TotalRecordCount"] = context.TotalRecordCount.ToString(),
                ["TotalTabs"] = context.TotalTabs.ToString()
            };
            //All header elements should start with "X-".
            headerData = headerData.Concat(context.ImportHeaderInfo)
                    .ToDictionary(x => "X-Import-" + x.Key, x => x.Value);
            return headerData;

        }

        private async Task<List<ApiResult>> CallApiAsync(ImportContext context,
            IEnumerable<PayloadData> payloadDataItems, IDictionary<string, string> headerData
            )
        {
            int maxConcurrentAsyncApiCalls;
            int.TryParse(ConfigurationManager.AppSettings["MaxConcurrentAsyncApiCalls"], out maxConcurrentAsyncApiCalls);

            _logger.Debug($"Value of maxConcurrentAsyncApiCalls is : {maxConcurrentAsyncApiCalls} ");
            if (maxConcurrentAsyncApiCalls == 0)
            {
                maxConcurrentAsyncApiCalls = 4;
                _logger.Debug($"Using Default Value of maxConcurrentAsyncApiCalls : {maxConcurrentAsyncApiCalls} ");
            }

            var payLoadItemsList = payloadDataItems.SplitItemsList(maxConcurrentAsyncApiCalls);

            IEnumerable<ApiResult> apiResults = new List<ApiResult>();
            if (payLoadItemsList == null) return apiResults.ToList();
            foreach (var payloadItems in payLoadItemsList)
            {
                var apiResult = await Task.WhenAll(payloadItems.Select(t => _apiExecutor.ExecuteApiAsync(Guid.Parse(context.MasterSessionId),
                    t, headerData)));
                apiResults = apiResults.Concat(apiResult);
            }
            return apiResults.ToList();
        }

    }
}
