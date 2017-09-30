using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public class NonBatchPayloadPreparer : PayloadPreparer, IPayloadExtracter
    {
        private readonly IApiRecordJsonGenerator _apiRecordJsonGenerator;
        private readonly IGenerateFailedRecord _generateFailedRecord;

        public PreparePayloadTypeEnum GetPreparePayloadType()
        {
            return PreparePayloadTypeEnum.NonBatch;
        }
        public NonBatchPayloadPreparer(IRouteParameterFormatter routeParameterFormatter, ILog logger,
            IApiRecordJsonGenerator apiRecordJsonGenerator, IGenerateFailedRecord generateFailedRecord, ICalculate calculate) : 
            base(routeParameterFormatter, logger)
        {
            _generateFailedRecord = generateFailedRecord;
            _apiRecordJsonGenerator = apiRecordJsonGenerator;
        }
        public List<PayloadData> PreparePayload(ImportContext context, IEnumerable<ApiRecord> apiRecords)
        {
            var payloadDataItems = new List<PayloadData>();
            ErrorResultDataItems = new List<ErrorResultData>();
            var mapping = context.ApiMapping.FirstOrDefault();
            foreach (var apiRecord in apiRecords)
            {             
                try
                {
                    var verb = apiRecord.Record.GetVerb();                   
                    var endpoints = RouteParameterFormatter.FormatAllEndPointsWithParamValue(context.Endpoints,
                        apiRecord.Record);
                    var apiEndpointWithVerb = verb.GetEndPointWithVerb(endpoints);
                    if (ShouldVerbChangedToPatch(apiEndpointWithVerb.Key, endpoints))
                    {
                        apiEndpointWithVerb = new KeyValuePair<HtmlVerb, string>(HtmlVerb.Patch, endpoints[HtmlVerb.Patch]);
                    }
                    CheckIfEndPointIsValidAndRemoveExceptionMessage(apiRecord, apiEndpointWithVerb.Value,
                        apiEndpointWithVerb.Key, mapping);
                    payloadDataItems.Add(new PayloadData
                    {
                        EndPoint = apiEndpointWithVerb.Value,
                        HtmlVerb = apiEndpointWithVerb.Key,
                        PayLoad = GetPayload(apiRecord),
                        ApiRecord = apiRecord
                    });
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    var message =
                        $"Action Type '{invalidOperationException.Message}' is not supported &ndash; Valid values A=Add, C=Change, D=Delete, U=Upsert";
                    Logger.Error(message, invalidOperationException);
                    var errorResponse = new ErrorResponse { Detail = message };
                    AddErrorResultData(errorResponse, apiRecord);
                }
                catch (EndpointNotFoundException endpointNotFoundException)
                {
                    Logger.Error($"An Error Occurred in {nameof(NonBatchPayloadPreparer)}:{nameof(PreparePayload)} for payload", endpointNotFoundException);
                    Logger.Debug(apiRecord.Record);
                    var errorResponse = new ErrorResponse { Detail = endpointNotFoundException.Message};
                    AddErrorResultData(errorResponse, apiRecord);
                }
                catch (Exception exception)
                {
                    Logger.Error($"An Error Occurred in {nameof(NonBatchPayloadPreparer)}:{nameof(PreparePayload)} for payload", exception);
                    Logger.Debug(apiRecord.Record);
                    var errorResponse = new ErrorResponse { Detail = exception.Message };
                    AddErrorResultData(errorResponse, apiRecord);
                }
            }
            return payloadDataItems;
        }

        private void AddErrorResultData(ErrorResponse errorResponse, ApiRecord apiRecord)
        {
            var errorResultData = new ErrorResultData
            {
                ErrorResponse = errorResponse,
                FailedRecord = _generateFailedRecord.GetFailedRecord(apiRecord, errorResponse, null),
                HttpExporterResult = null,
                RowNumber = apiRecord.RowNumber,
                ImportType = apiRecord.ImportType
            };
            ErrorResultDataItems.Add(errorResultData);
        }

        private string GetPayload(ApiRecord apiRecord)
        {
            apiRecord.Record = RouteParameterFormatter.RemoveLookupParameters(apiRecord.Record);
            var jsonData = _apiRecordJsonGenerator.SerializeRecordJson(apiRecord.Record);
            jsonData = _apiRecordJsonGenerator.MergeJson(apiRecord, jsonData);
            return jsonData;
        }

    
    }
}
