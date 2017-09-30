using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Transformers
{
    public class LookupResolver : ILookupResolver<MappingFieldDefinition>
    {
        private readonly ILog _log;
        private readonly IHttpInvoker _httpInvoker;
        private readonly IRouteParameterFormatter _routeParameterFormatter;

        public LookupResolver(ILog log, IHttpInvoker httpInvoker, 
            IRouteParameterFormatter routeParameterFormatter)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(httpInvoker, nameof(httpInvoker));
            Ensure.ThatArgumentIsNotNull(routeParameterFormatter,nameof(routeParameterFormatter));
            _log = log;
            _httpInvoker = httpInvoker;
            _routeParameterFormatter = routeParameterFormatter;
        }

        public Lookupvalues RetrieveLookupValue(MappingFieldDefinition mappingFieldDefinition, string masterSessionId, 
            IEnumerable<KeyValuePair<string, string>> record, ILookup lookup)
        {
            var preparedUri = string.Empty;
            var lookupValues = new Lookupvalues();
            try
            {
                
                preparedUri = PrepareUri(record, mappingFieldDefinition.Source.GetFieldsFromBraces(), mappingFieldDefinition, lookupValues);
                var responseContent = GetResponseContent(masterSessionId, preparedUri,lookup);

                //Either the verb is POST or the user is trying to put or delete for a non existing record.
                if (responseContent == ImportConstants.EmptyStringJsonArray || responseContent == null)
                {
                    lookup?.Remove(preparedUri);
                    lookupValues.Lookupvalue = string.Empty;
                    return lookupValues;
                }
   
                var responseObj = JToken.Parse(responseContent);
                lookupValues.Lookupvalue =
                    (string)responseObj.Children().FirstOrDefault()?.SelectToken(mappingFieldDefinition.ValuePath);
                _log.Debug($"Value for {mappingFieldDefinition.Destination.RemoveBraces()} is {lookupValues.Lookupvalue} by calling URL {preparedUri}");
            }
            catch (FormatException e)
            {
                _log.Error($"Exception occurred with message: {e.Message} in record",e);
                _log.Debug(record);
                lookup?.Remove(preparedUri);
                lookupValues.Lookupvalue = string.Empty;
                return lookupValues;
            }
            catch (Exception e)
            {
                _log.Error($"Exception thrown by the API, while calling httpclient.Get({preparedUri})", e);
                lookup?.Remove(preparedUri);
                lookupValues.Lookupvalue = string.Empty;
                return lookupValues;
            }
            
            return lookupValues;
        }

        private string GetResponseContent(string masterSessionId, string preparedUri, ILookup lookup)
        {
            var storedResponseContent = lookup?.Retrieve(preparedUri);

            if (!string.IsNullOrEmpty(storedResponseContent))
            {
                _log.Debug($"Using the cached response for Uri:{preparedUri}");
                return storedResponseContent;
            }
            var responseContent = CallApi(masterSessionId, preparedUri);
            lookup?.Store(preparedUri,responseContent);
            return responseContent;
        }

        private string CallApi(string masterSessionId, string preparedUri)
        {
            var response = _httpInvoker.CallApiEndpoint(Guid.Parse(masterSessionId), null, preparedUri, HtmlVerb.Get);
            var responseContent = response?.Response?.Content?.ReadAsStringAsync().Result;
            return responseContent;
        }

        private string PrepareUri(IEnumerable<KeyValuePair<string, string>> recordKeyValuePairs, IEnumerable<string> lookupParameters, MappingFieldDefinition mappingFieldDefinition, Lookupvalues lookupValues)
        {
            var emptyFieldNames = string.Empty;
            var lookupParamValues = new Dictionary<string, string>();
            var lookupParameterValues = new List<string>();
            var kvpRecords = recordKeyValuePairs as KeyValuePair<string, string>[] ?? recordKeyValuePairs.ToArray();
            foreach (var lookupParam in lookupParameters)
            {
                foreach (var recordKvp in kvpRecords)
                {
                    if (!recordKvp.Key.Equals(lookupParam, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    lookupParamValues[recordKvp.Key] = Uri.EscapeDataString(recordKvp.Value);

                    if (string.IsNullOrWhiteSpace(recordKvp.Value))
                    {
                        emptyFieldNames = emptyFieldNames + recordKvp.Key + ImportConstants.Comma;
                        lookupParameterValues.Add(recordKvp.Key + ":" + " Not provided.");
                    }
                    else
                    {
                        lookupParameterValues.Add(recordKvp.Key + ":" + recordKvp.Value);
                    }
                }
            }
            lookupValues.LookupParameterValues = lookupParameterValues;
            var uri = mappingFieldDefinition.EndPoint.ReplaceQueryStringParam(lookupParamValues);

            if (uri.ContainsOpenAndClosedBraces()) 
            {
                uri = _routeParameterFormatter.FormatEndPointWithParamValue(uri, kvpRecords.ToDictionary(x => x.Key, y => y.Value));
            }
            if (!lookupParamValues.Values.Any(string.IsNullOrWhiteSpace))
            {
                return uri;
            }

            _log.Error($"One or more replacement parameter value for the lookup URL is empty. URI=({uri}) for payload");
            emptyFieldNames = emptyFieldNames.Remove(emptyFieldNames.Length - 1);
            throw new FormatException($"Empty Values Detected for one or more fields: {emptyFieldNames}");
        }

    }
}
