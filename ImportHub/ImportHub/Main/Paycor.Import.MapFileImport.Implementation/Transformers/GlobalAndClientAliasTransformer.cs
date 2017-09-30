using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Transformers
{
    public class GlobalAndClientAliasTransformer : ITransformAliasRecordFields<MappingDefinition>
    {
        public class ValuePair
        {
            public string ClientId { get; set; }
            public string DataFileValue { get; set; }
            public string SubstitutionValue { get; set; }
        }

        public class AliasData
        {
            public string ClientId;
            public string Type;
            public string Key;
            public string Value;

            public override string ToString()
            {
                return $"ClientId:{ClientId}, Type:{Type}, Key:{Key}, Value: {Value}";
            }
        }

        private readonly ILog _logger;
        private readonly IHttpInvoker _httpInvoker;
        private readonly string _lookupUri;
        private const string CamelClientId = "clientId";
        public GlobalAndClientAliasTransformer(ILog logger, IHttpInvoker httpInvoker, string lookupUri)
        {
            _logger = logger;
            _httpInvoker = httpInvoker;
            _lookupUri = lookupUri;
        }

        public IEnumerable<IEnumerable<KeyValuePair<string, string>>> TransformAliasRecordFields(MappingDefinition mappingDefinition,
            IEnumerable<IEnumerable<KeyValuePair<string, string>>> records, string masterSessionId)
        {
            IDictionary<ValuePair, string> allClientsLookupKeyValuePairs = new Dictionary<ValuePair, string>();
            var kvpRecords = records as IList<IEnumerable<KeyValuePair<string, string>>> ?? records.ToList();
            try
            {
                var listOfClientIds = new List<string>();
                var transformRecords = new List<IEnumerable<KeyValuePair<string, string>>>();
                var aliasDictionary = new Dictionary<string, string>();
                foreach (var record in kvpRecords)
                {
                    var kvpRecord = record as KeyValuePair<string, string>[] ?? record.ToArray();
                    var clientId = (from kvp in kvpRecord where kvp.Key.Equals(CamelClientId, StringComparison.OrdinalIgnoreCase) select kvp.Value).ToList().FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(clientId))
                    {
                        _logger.Warn("Client Id not found in the file. Skipping Alias transformation.");
                        transformRecords.Add(kvpRecord);
                        continue;
                    }
                    if (!listOfClientIds.Contains(clientId))
                    {
                        IEnumerable<JToken> responseObj;
                        var isSuccess = GetLookupValuesFromApi(masterSessionId, clientId, out responseObj);
                        if (isSuccess)
                        {
                            listOfClientIds.Add(clientId);
                            allClientsLookupKeyValuePairs = allClientsLookupKeyValuePairs.Concat(ParseLookupValues(responseObj, clientId)).ToDictionary(x=> x.Key, y=> y.Value);
                        }                        
                    }
                    var recordTransformed = TransformRecords(kvpRecord, allClientsLookupKeyValuePairs, clientId, aliasDictionary);
                    transformRecords.Add(recordTransformed);
                }
                return transformRecords;
            }
            catch (Exception exception)
            {
                _logger.Error($"An Error Occurred in {nameof(GlobalAndClientAliasTransformer)}:{nameof(TransformAliasRecordFields)} ", exception);
                return kvpRecords;
            }
        }

        private IEnumerable<KeyValuePair<string, string>> TransformRecords(
            IEnumerable<KeyValuePair<string, string>> record,
            IDictionary<ValuePair, string> allClientsLookupKeyValuePairs, string clientId, Dictionary<string,string> aliasDictionary)
        {
            var clientLookupKeyValuePairs = allClientsLookupKeyValuePairs.Where(t => t.Key.ClientId == clientId || t.Key.ClientId == "0").ToDictionary(x=>x.Key, y=> y.Value);
            if (!clientLookupKeyValuePairs.Any())
                return record;
            
                var kvpRecord = (IList<KeyValuePair<string, string>>)record;
                var transformedRecord = new List<KeyValuePair<string, string>>();
               return TransformColumnsinRecord(clientLookupKeyValuePairs, kvpRecord, transformedRecord, aliasDictionary);
                    
        }

        private ICollection<KeyValuePair<string, string>> TransformColumnsinRecord(
            IDictionary<ValuePair, string> allClientsLookupKeyValuePairs,
            IList<KeyValuePair<string, string>> record,
            ICollection<KeyValuePair<string, string>> transformedRecord, Dictionary<string, string> aliasDictionary)
        {
            try
            {
                
                foreach (var data in record)
                {
                    var lookupSubstituteValue = string.Empty;
                    var lookupType = data.Key.GetSuffixAfterSymbol(ImportConstants.Period);
                    var lookupKey = data.Value;

                    if (allClientsLookupKeyValuePairs.Any())
                    {
                        lookupSubstituteValue = LookupAllClientsData(allClientsLookupKeyValuePairs,
                            lookupSubstituteValue, lookupType, lookupKey);                                              
                    }
                    if (!string.IsNullOrWhiteSpace(lookupSubstituteValue))
                    {
                        if (!aliasDictionary.ContainsKey(lookupKey))
                        {
                            aliasDictionary[lookupKey] = lookupSubstituteValue;
                            _logger.Info($"Substituted value for Type: {data.Key}, for thirdPartyValue {lookupKey} to {lookupSubstituteValue}");
                        }
                    } 
                    transformedRecord.Add(new KeyValuePair<string, string>(data.Key, string.IsNullOrEmpty(lookupSubstituteValue) 
                                                                                        ? lookupKey 
                                                                                        : lookupSubstituteValue));
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An Error Occurred in {nameof(GlobalAndClientAliasTransformer)}:{nameof(TransformColumnsinRecord)} action", e);
                return record;
            }
            return transformedRecord;
        }

        private static string LookupAllClientsData(IDictionary<ValuePair, string> allClientsLookupKeyValuePairs,
                    string lookupSubstituteValue, string lookupType, string lookupKey)
        {
            foreach (var clientKeyValue in allClientsLookupKeyValuePairs)
            {
                lookupSubstituteValue = GetLookupSubstituteValue(clientKeyValue, lookupType, lookupKey);
                if (lookupSubstituteValue.Length > 0)
                    break;
            }
            return lookupSubstituteValue;
        }

        private bool GetLookupValuesFromApi(string masterSessionId, string clientId, out IEnumerable<JToken> responseObj)
        {
            responseObj = new List<JToken>();
            var lookupUri = PrepareUri(clientId);
            if (null == _lookupUri)
            {
                return false;
            }
            try
            {
                var response = _httpInvoker.CallApiEndpoint(Guid.Parse(masterSessionId), null, lookupUri, HtmlVerb.Get);
                if (!response.IsSuccess)
                {
                    _logger.Warn($"Call to Alias Manager failed with status {response?.Response?.StatusCode}");
                    return false;
                }
                _logger.Info($"Alias call was sucess for client {clientId} with status {response?.Response?.StatusCode}");
                var responseContent = response.Response.Content != null
                    ? response.Response.Content.ReadAsStringAsync().Result
                    : "No response provided";
                try
                {
                    var aliasesData = JsonConvert.DeserializeObject<AliasData[]>(responseContent);                   
                    foreach (var aliasData in aliasesData?.Take(10))
                    {
                        _logger.Info($"Alias data is {aliasData}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn("Cannot convert the response content to alias[]", ex);
                    //eat it
                }                
                _logger.Debug($"Master Session Id is: {masterSessionId}");
                responseObj = JToken.Parse(responseContent).Children();                              
            }
            catch (Exception e)
            {
                _logger.Error($"Exception occurred while calling the httpclient.Get({lookupUri})",e);
                return false;
            }
            return true;
        }

        private string PrepareUri(string clientId)
        {
            var lookupUri = _lookupUri.ReplaceRouteParamWithValue(ImportConstants.Client.AddBraces(), clientId);
            return lookupUri;
        }

        private static IDictionary<ValuePair, string> ParseLookupValues(IEnumerable<JToken> result, string clientId)
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
                    ClientId = clientId,
                    DataFileValue = key,
                    SubstitutionValue = value
                };

                lookupKeyValuePairs.Add(valuePair, type);
            }
            return lookupKeyValuePairs;
        }

        private static string GetValue(JProperty property)
        {
            return property.Value.ToString();
        }

        private static string GetLookupSubstituteValue(KeyValuePair<ValuePair, string> input, string apiFieldDestination,
            string key)
        {
            var lookupSubstituteValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(input.Value) && input.Value.RemoveWhiteSpaces().Equals(apiFieldDestination.RemoveWhiteSpaces(), StringComparison.OrdinalIgnoreCase)) 
            {
                lookupSubstituteValue = input.Key.DataFileValue.RemoveWhiteSpaces().Equals(key.RemoveWhiteSpaces(), StringComparison.OrdinalIgnoreCase) ? input.Key.SubstitutionValue : string.Empty;
            }
            return lookupSubstituteValue;
        }        
    }
}
