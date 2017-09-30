using System.Collections.Generic;
using Paycor.Import.Messaging;

namespace Paycor.Import.Mapping
{
    public interface IRouteParameterFormatter
    {
        string FormatEndPointWithParamValue(string apiEndpoint, IDictionary<string, string> record);
        IDictionary<string, string> RemoveLookupParameters(IDictionary<string, string> record);
        bool HasLookupParameters(IDictionary<string, string> record);
        Dictionary<HtmlVerb, string> FormatAllEndPointsWithParamValue(IDictionary<HtmlVerb, string> endpoints,IDictionary<string, string> record);
        bool IsEndPointInvalidForOperation(string endPoint);
    }
}