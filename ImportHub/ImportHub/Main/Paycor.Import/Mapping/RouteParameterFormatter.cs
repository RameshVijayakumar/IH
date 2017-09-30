using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.Messaging;
//TODO: Missing unit tests

namespace Paycor.Import.Mapping
{
    public class RouteParameterFormatter : IRouteParameterFormatter
    {
        public string FormatEndPointWithParamValue(string apiEndpoint, IDictionary<string, string> record)
        {
            if (apiEndpoint == null || apiEndpoint.CountofLookupParameters() <= 0)
                return apiEndpoint;

            foreach (var parameter in record)
            {
                apiEndpoint = apiEndpoint.ReplaceRouteParamWithValue(parameter.Key.AddBraces(), parameter.Value);
                if (null == apiEndpoint)
                {
                    throw new Exception("Unable to replace RouteParameters for EndPoint, possible empty route parameter");
                }
            }
            return apiEndpoint;
        }

        public IDictionary<string, string> RemoveLookupParameters(IDictionary<string, string> record)
        {
            return HasLookupParameters(record) ? record.Where(t => t.Key != null && t.Key.IsLookupParameter() != true).Select(t => t).ToDictionary(t => t.Key, t => t.Value) : record;
        }

        public bool HasLookupParameters(IDictionary<string, string> record)
        {
            return record != null && record.Count(t => t.Key.IsLookupParameter()) > 0;
        }

        public Dictionary<HtmlVerb, string> FormatAllEndPointsWithParamValue(IDictionary<HtmlVerb, string> endpoints, IDictionary<string, string> record)
        {
            var formattedEndpoints = new Dictionary<HtmlVerb,string>();
            foreach (var endpoint in endpoints)
            {
                formattedEndpoints[endpoint.Key] = FormatEndPointWithParamValue(endpoint.Value, record);
            }
            return formattedEndpoints;
        }

        public bool IsEndPointInvalidForOperation(string endPoint)
        {
            if (string.IsNullOrWhiteSpace(endPoint)) return true;
            return endPoint.ContainsOpenAndClosedBraces();
        }
    }
}