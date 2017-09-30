using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public class CertifyMappingLookupFields : IVerifyMaps
    {
        private readonly ILog _log;

        public CertifyMappingLookupFields(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
        }
        public IEnumerable<GeneratedMapping> CertifyMaps(IEnumerable<GeneratedMapping> apiMappings)
        {
            var lookupNotCertifiedMaps = new List<GeneratedMapping>();
            foreach (var map in apiMappings)
            {
                var sourceFields = map.Mapping.GetAllSourceFieldsWithoutLookupAndConst();
                var lookupSourceFields = map.Mapping.GetAllLookupSourceFields();
                if (!lookupSourceFields.Any()) continue;
                if (!IsLookupFieldsAreMappable(sourceFields, lookupSourceFields))
                {
                    lookupNotCertifiedMaps.Add(map);
                    _log.Error($"#RegistrationFailed: Lookup fields {lookupSourceFields.ToList().ConcatListOfString(",")} of Import type {map.MappingName} are not mappable.");
                    continue;
                }
                if (IsLookupSourcePresentInLookupEndPoint(map)) continue;
                lookupNotCertifiedMaps.Add(map);
                _log.Error($"#RegistrationFailed:Lookup fields {lookupSourceFields.ToList().ConcatListOfString(",")} of Import type {map.MappingName} are not the same as the endpoint of lookup field definition.");
                
            }
            return apiMappings.Except(lookupNotCertifiedMaps);
        }

        private static bool IsLookupFieldsAreMappable(IEnumerable<string> sourceFields,
            IEnumerable<string> lookupSourceFields)
        {
            return sourceFields.Any() && lookupSourceFields.ExistIn(sourceFields);
        }

        private static bool IsLookupSourcePresentInLookupEndPoint(GeneratedMapping map)
        {
            var lookupFieldDefinition =
                map.Mapping.FieldDefinitions.Where(t => t.Source.ContainsOpenAndClosedBraces());
            return lookupFieldDefinition.All(fieldDefinition => fieldDefinition.EndPoint.Contains(fieldDefinition.Source));
        }
    }
}
