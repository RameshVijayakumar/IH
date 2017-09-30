using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.Extensions
{
    public static class MappingEndpointsExtensions
    {
        public static IEnumerable<string> GetListOfAllMappingEndpoints(this MappingEndpoints mappingEndpoints)
        {
            var allEndpoints = new List<string>();
            if (mappingEndpoints == null)
                return allEndpoints;

            if(mappingEndpoints.Post != null)
                allEndpoints.Add(mappingEndpoints.Post);

            if (mappingEndpoints.Patch != null)
                allEndpoints.Add(mappingEndpoints.Patch);

            if (mappingEndpoints.Put != null)
                allEndpoints.Add(mappingEndpoints.Put);

            if (mappingEndpoints.Delete != null)
                allEndpoints.Add(mappingEndpoints.Delete);

            return allEndpoints;
        }

        public static IList<string> GetListOfOptedOutEndPointsInfo(this MappingEndpoints mappingEndpoints)
        {
            var optedOutEndPointsInfo = new List<string>();
            if (mappingEndpoints == null)
                return optedOutEndPointsInfo;

            if (mappingEndpoints.Put == null && mappingEndpoints.Patch == null)
            {
                optedOutEndPointsInfo.Add("Put");
                optedOutEndPointsInfo.Add("Patch");
            }
            else if(mappingEndpoints.Put != null && mappingEndpoints.Patch != null)
            {
                optedOutEndPointsInfo.Add("Patch and Put both are opted-In. Patch will be the preffered operation");
            }
            else if(mappingEndpoints.Put != null && mappingEndpoints.Patch == null)
            {
                optedOutEndPointsInfo.Add("Patch");
            }
            else if (mappingEndpoints.Put == null && mappingEndpoints.Patch != null)
            {
                optedOutEndPointsInfo.Add("Put");
            }

            if (mappingEndpoints.Delete == null)
                optedOutEndPointsInfo.Add("Delete");

            return optedOutEndPointsInfo;
        }
    }
}
