using System;
using System.Collections.Generic;
using System.ServiceModel;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public class PayloadPreparer
    {
        protected readonly IRouteParameterFormatter RouteParameterFormatter;
        protected readonly ILog Logger;

        public PayloadPreparer(IRouteParameterFormatter routeParameterFormatter, ILog logger)
        {
            Ensure.ThatArgumentIsNotNull(routeParameterFormatter, nameof(routeParameterFormatter));
            RouteParameterFormatter = routeParameterFormatter;
            Logger = logger;
        }

        protected List<ErrorResultData> ErrorResultDataItems;

        public List<ErrorResultData> GetErrorResultItems()
        {
            return ErrorResultDataItems;
        }

        protected static bool ShouldVerbChangedToPatch(HtmlVerb verb, IReadOnlyDictionary<HtmlVerb, string> endpoints)
        {
            return verb == HtmlVerb.Put && (endpoints[HtmlVerb.Patch] != null);
        }

        protected void CheckIfEndPointIsValidAndRemoveExceptionMessage(ApiRecord apiRecord, string apiEndpoint, HtmlVerb verb, ApiMapping mapping)
        {
            if (apiRecord.Record == null)
            {
                return;
            }
            if (apiEndpoint == null)
            {
                Logger.Error($"Action {verb} is not supported by the destination API");
                throw new EndpointNotFoundException($"{verb.GetActionFromVerb()} is not supported for this import type");
            }
            string exceptionMessage;
            apiRecord.Record.TryGetValue(ImportConstants.LookUpRouteExceptionMessageKey, out exceptionMessage);
            apiRecord.RemoveKeyFromRecord(ImportConstants.LookUpRouteExceptionMessageKey);
            if (RouteParameterFormatter.IsEndPointInvalidForOperation(apiEndpoint))
            {
                Logger.Error($"Invalid endpoint: {apiEndpoint} to call {verb}");
                if (string.IsNullOrWhiteSpace(exceptionMessage))
                {
                    exceptionMessage = GetExceptionMessage(mapping, apiEndpoint);
                }
                throw new EndpointNotFoundException(exceptionMessage);
            }
            if (!apiRecord.IsPayloadMissing)
            {
                Logger.Debug($"No Missing payload for importing the record: {apiRecord.Record}");
                return;
            }
            Logger.Error($"Missing payload for importing the record: {apiRecord.Record}");
            if (exceptionMessage == null)
                exceptionMessage = "Record cannot be imported as, one or more data required for the import is missing.";
            throw new EndpointNotFoundException(exceptionMessage);
        }

        private string GetExceptionMessage(ApiMapping mapping, string endPoint)
        {
            try
            {
                var routeParams = endPoint.GetFieldsFromBraces();
                var requiredFields = mapping.GetAllDestRequiredFields();
                return routeParams.ExistIn(requiredFields) ? "Record cannot be imported as, one or more data required for the import is missing:" + routeParams.ConcatListOfString(",") : "Record cannot be imported as, one or more data required for the import is missing.";
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred at {nameof(GetExceptionMessage)}", ex);
                return "Record cannot be imported as, one or more data required for the import is missing.";
            }          
        }
    }
}