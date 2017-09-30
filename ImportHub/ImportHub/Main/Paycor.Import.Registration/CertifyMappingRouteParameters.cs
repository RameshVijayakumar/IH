using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public class CertifyMappingRouteParameters : IVerifyMaps
    {
        private readonly ILog _log;

        public CertifyMappingRouteParameters(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
        }
        public IEnumerable<GeneratedMapping> CertifyMaps(IEnumerable<GeneratedMapping> apiMappings)
        {

            var routeParametersNotCertifiedMaps = new List<GeneratedMapping>();
            var mappings = apiMappings as GeneratedMapping[] ?? apiMappings.ToArray();
            foreach (var map in mappings)
            {
                var mappingEndPoints = new List<string>();

                var combinedLookupDestAndRequredFields = new List<string>();
                var lookupDestination = map.Mapping.GetAllDestinationFieldsWhichAreLookups().ToList();
                var requiredFields = map.GetAllRequiredFields();
                combinedLookupDestAndRequredFields.AddRange(lookupDestination);
                combinedLookupDestAndRequredFields.AddRange(requiredFields);

                mappingEndPoints.AddRange(map.MappingEndpoints.GetListOfAllMappingEndpoints());
                mappingEndPoints.AddRange(map.GetLookupEndpointsWithoutQueryString());
                var validEndPoints = true;
                foreach (var endPoint in mappingEndPoints)
                {
                    var routeParamerters = endPoint.GetFieldsFromBraces();
                    if (!routeParamerters.Any()) continue;
                    if (IsRouteParameterResolvedByLookup(routeParamerters, lookupDestination)) continue;
                    if (IsRouteParameterRequiredFieldInFieldDefinition(routeParamerters, requiredFields)) continue;
                    if(IsRouteParameterPresentInRequiredFieldAndLookupDest(routeParamerters, combinedLookupDestAndRequredFields)) continue;

                    _log.Error($"#RegistrationFailed No lookup or required field exists for one or more route parameters {routeParamerters.ConcatListOfString(",")} in {endPoint}");
                    validEndPoints = false;
                    break;
                }
                if (validEndPoints) continue;
                _log.Error($"#RegistrationFailed Import type {map.MappingName} does not satisfy the route parameters through lookup or required fields.");
                routeParametersNotCertifiedMaps.Add(map);
            }
            return mappings.Except(routeParametersNotCertifiedMaps);
        }

        private static bool IsRouteParameterResolvedByLookup(IList<string> routeParameters, IList<string> lookupDestination)
        {
            return lookupDestination.Any() && routeParameters.ExistIn(lookupDestination);
        }

        private static bool IsRouteParameterRequiredFieldInFieldDefinition(IList<string> routeParameters,
            IList<string> requiredFields)
        {
            return requiredFields.Any() && routeParameters.ExistIn(requiredFields);
        }

        private static bool IsRouteParameterPresentInRequiredFieldAndLookupDest(IList<string> routeParameters,
            IList<string> combinedLookupDestAndRequredFields)
        {
            return combinedLookupDestAndRequredFields.Any() &&
                   routeParameters.ExistIn(combinedLookupDestAndRequredFields);
        }
    }
}
