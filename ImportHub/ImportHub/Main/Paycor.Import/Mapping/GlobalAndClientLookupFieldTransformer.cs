using System;
using System.Linq;
using System.Collections.Generic;
using log4net;
using Paycor.Import.Http;
using Paycor.Import.Extensions;
using Paycor.Import.Messaging;
using Newtonsoft.Json.Linq;
//TODO: No unit tests

namespace Paycor.Import.Mapping
{
    public class GlobalAndClientLookupFieldTransformer : ITransformFields<MappingDefinition>
    {
        public class ValuePair
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private readonly ILog _log;
        private readonly IHttpInvoker _httpInvoker;
        private readonly string _lookupUri;

        public GlobalAndClientLookupFieldTransformer(ILog log, IHttpInvoker httpInvoker, string lookupUri)
        {
            _log = log;
            _httpInvoker = httpInvoker;
            _lookupUri = lookupUri;
        }

        public IDictionary<string, string> TransformFields(MappingDefinition mappingDefinition, IDictionary<string, string> record, string masterSessionId = null)
        {
            IDictionary<ValuePair, string> clientLookupKeyValuePairs = new Dictionary<ValuePair, string>();
            var transformedRecord = new Dictionary<string, string>();

            var clientId = GetClientId(record);
            if (!string.IsNullOrEmpty(clientId))
            {
                clientLookupKeyValuePairs =
                    ParseLookupValues(GetLookupValuesFromApi(masterSessionId, clientId));
            }

            var globalLookupKeyValuePairs =
                ParseLookupValues(GetLookupValuesFromApi(masterSessionId, ImportConstants.GlobalClientId));

            foreach (var data in record)
            {
                var lookupSubstituteValue = string.Empty;
                var lookupType = data.Key.GetSuffixAfterSymbol(ImportConstants.Period);
                var lookupKey = data.Value;

                if ((null != clientLookupKeyValuePairs) && clientLookupKeyValuePairs.Any())
                {
                    foreach (var clientKeyValue in clientLookupKeyValuePairs)
                    {
                        lookupSubstituteValue = GetLookupSubstituteValue(clientKeyValue, lookupType, lookupKey);
                        if (lookupSubstituteValue.Length > 0)
                            break;
                    }
                }
                if (string.IsNullOrEmpty(lookupSubstituteValue))
                {
                    if ((null != globalLookupKeyValuePairs) && globalLookupKeyValuePairs.Any())
                    {
                        foreach (var globalKeyValue in globalLookupKeyValuePairs)
                        {
                            lookupSubstituteValue = GetLookupSubstituteValue(globalKeyValue, lookupType, lookupKey);
                            if (lookupSubstituteValue.Length > 0)
                                break;
                        }
                    }
                }
                transformedRecord.Add(data.Key, string.IsNullOrEmpty(lookupSubstituteValue)
                    ? lookupKey
                    : lookupSubstituteValue);
            }
            return transformedRecord;
        }

        private IEnumerable<JToken> GetLookupValuesFromApi(string masterSessionId, string clientId)
        {
            var lookupUri = PrepareUri(clientId);
            if (null == _lookupUri)
            {
                return null;
            }

            IEnumerable<JToken> responseObj = new List<JToken>();
            try
            {
                var response = _httpInvoker.CallApiEndpoint(Guid.Parse(masterSessionId), null, lookupUri, HtmlVerb.Get);
                var responseContent = response.Response.Content != null
                    ? response.Response.Content.ReadAsStringAsync().Result
                    : "No response provided";

                responseObj = JToken.Parse(responseContent).Children();
            }
            catch (Exception e)
            {
                _log.Error($"Exception occurred while calling the httpclient.Get({lookupUri})",e);
            }
            return responseObj;
        }

        private string PrepareUri(string clientId)
        {
            var lookupUri = _lookupUri.ReplaceRouteParamWithValue(ImportConstants.Client.AddBraces(), clientId);
            return lookupUri;
        }

        private static IDictionary<ValuePair, string> ParseLookupValues(IEnumerable<JToken> result)
        {
            if (null == result)
                return null;

            var lookupKeyValuePairs = new Dictionary<ValuePair, string>();

            foreach (var jObjProps in result.Select(jObj => jObj.Values<JProperty>()))
            {
                var type = string.Empty;
                var key = string.Empty;
                var value = string.Empty;

                foreach (var prop in jObjProps)
                {
                    switch (prop.Name.ToLower())
                    {
                        case ImportConstants.Type:
                            type = GetValue(prop);
                            break;
                        case ImportConstants.Key:
                            key = GetValue(prop);
                            break;
                        case ImportConstants.Value:
                            value = GetValue(prop);
                            break;
                    }
                }
                var valuePair = new ValuePair
                {
                    Key = key,
                    Value = value
                };

                lookupKeyValuePairs.Add(valuePair, type);
            }
            return lookupKeyValuePairs;
        }

        private static string GetValue(JProperty property)
        {
            return property.Value.ToString();
        }

        private static string GetLookupSubstituteValue(KeyValuePair<ValuePair, string> input, string substring, string key)
        {
            var lookupSubstituteValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(input.Value) && input.Value.ContainsSubstring(substring))
            {
                lookupSubstituteValue = input.Key.Key.Equals(key, StringComparison.OrdinalIgnoreCase)
                    ? input.Key.Value
                    : string.Empty;
            }
            return lookupSubstituteValue;
        }

        private static string GetClientId(IDictionary<string, string> record)
        {
            var clientId = string.Empty;
            foreach (var rec in record)
            {
                if (!rec.Key.Equals(ImportConstants.Client, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(clientId))
                {
                    clientId = rec.Value;
                }
                else
                {
                    break;
                }
            }
            return clientId;
        } 
    }
}
