using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public class MappingLoggingInfo : IVerifyMaps
    {
        private readonly ILog _log;

        public MappingLoggingInfo(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
        }
        public IEnumerable<GeneratedMapping> CertifyMaps(IEnumerable<GeneratedMapping> apiMappings)
        {
            var mappings = apiMappings as GeneratedMapping[] ?? apiMappings.ToArray();
            LogMappingOptedOutEndPointInfo(mappings);
            LogMappingRequiredFields(mappings);

            return mappings;
        }


        private void LogMappingRequiredFields(IEnumerable<GeneratedMapping> apiMappings)
        {
            foreach (var map in apiMappings)
            {
                var allRequiredFields = map.GetAllRequiredFields();
                if(!allRequiredFields.Any()) continue;
                var requiredFields = allRequiredFields.ConcatListOfString(",");
                _log.Info($"Import type {map.MappingName} has following required fields: {requiredFields}");
            }    
        }

        private void LogMappingOptedOutEndPointInfo(IEnumerable<GeneratedMapping> apiMappings)
        {
            foreach (var map in apiMappings)
            {
                var endPointsOptedOut = map.MappingEndpoints.GetListOfOptedOutEndPointsInfo();

                if (!endPointsOptedOut.Any()) continue;
                var endPointsOptedOutInfo = endPointsOptedOut.ConcatListOfString(",");
                _log.Info($"Import Type {map.MappingName} has no endpoints for {endPointsOptedOutInfo}");
            }
        }
    }
}
