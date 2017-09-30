using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;

// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.MapFileImport.Implementation
{
    public class LookupRulesEvaluator : IRulesEvaluator
    {
        private readonly ILog _log;

        public LookupRulesEvaluator(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
        }
        public IEnumerable<MappingFieldDefinition> SortLookupOrder(IEnumerable<KeyValuePair<string, string>> record,
            MappingDefinition mappingDefinition, IEqualityComparer<MappingFieldDefinition> comparer)
        {
            Ensure.ThatArgumentIsNotNull(comparer, nameof(comparer));

            var allLookupDefs = mappingDefinition.GetAllLookupFieldDefinitions(record, comparer).ToList();
            var recordList = record.ToList();

            var lookupDefsWithNoDependency = GetLookupDefsWithNoDependency(allLookupDefs, recordList);
            var lookupDefsWithDependency = GetLookupDefsWithDependency(allLookupDefs, lookupDefsWithNoDependency, recordList);

            if (lookupDefsWithNoDependency.Any())
            {
                var sortedLookups = lookupDefsWithNoDependency.Concat(lookupDefsWithDependency).ToList();
                _log.Debug(sortedLookups);

                return sortedLookups;
            }
            
            return lookupDefsWithDependency;
        }

        public bool ValidateRules(MappingFieldDefinition mappingFieldDefinition, IEnumerable<KeyValuePair<string, string>> previouslyLookedupValues,
                 IEnumerable<string> listofRecordKeys)
        {
            if (mappingFieldDefinition?.EndPoint == null) return false;
            if (previouslyLookedupValues.RecordContainsKey(mappingFieldDefinition.Destination.RemoveBraces())) return false;
            var lookupSourceValues = mappingFieldDefinition.Source.GetFieldsFromBraces();
            if (lookupSourceValues.Count <= 1) return true;
            if (lookupSourceValues.ExistIn(listofRecordKeys.ToList())) return true;
            return false;
        }

        private IEnumerable<MappingFieldDefinition> GetLookupDefsWithDependency(IEnumerable<MappingFieldDefinition> allLookupDefs, IEnumerable<MappingFieldDefinition> lookupDefsWithNoDependency, List<KeyValuePair<string, string>> recordList)
        {
            var remainingLookupDefs = allLookupDefs.Except(lookupDefsWithNoDependency).ToList();

            var lookupDefsWithDependency = SortAndGetLookupFieldsAndAddLookupData(remainingLookupDefs, recordList);
            return lookupDefsWithDependency;
        }

        private List<MappingFieldDefinition> GetLookupDefsWithNoDependency(IEnumerable<MappingFieldDefinition> allLookupDefs, ICollection<KeyValuePair<string, string>> recordList)
        {
            var lookupDefsWithNoDependency = GetLookupFieldsAndAddLookupData(allLookupDefs, recordList);
            return lookupDefsWithNoDependency;
        }

        private List<MappingFieldDefinition> SortAndGetLookupFieldsAndAddLookupData
           (IEnumerable<MappingFieldDefinition> lookupFieldDefinitions,
           ICollection<KeyValuePair<string, string>> recordData)
        {
            var finalMappingFieldDefs = new List<MappingFieldDefinition>();
            var sortedLookupDefs = lookupFieldDefinitions.ToList();
            sortedLookupDefs.Sort();
            foreach (var lookupFieldDefinition in sortedLookupDefs)
            {
                var lookupSourceValues = lookupFieldDefinition.Source.GetFieldsFromBraces();
                var recordKeys = recordData.Where(t => !string.IsNullOrWhiteSpace(t.Value))
                    .Select(t => t.Key).ToList().Distinct();
                if (!lookupSourceValues.ExistIn(recordKeys)) continue;
                finalMappingFieldDefs.Add(lookupFieldDefinition);
                _log.Debug($" Sorted Lookup Order Source: {lookupFieldDefinition.Source}, Dest:{lookupFieldDefinition.Destination}");
                recordData.Add(new KeyValuePair<string, string>(lookupFieldDefinition.Destination.RemoveBraces(), "Data"));
            }
            return finalMappingFieldDefs;
        }



        private List<MappingFieldDefinition> GetLookupFieldsAndAddLookupData
            (IEnumerable<MappingFieldDefinition> lookupFieldDefinitions,
            ICollection<KeyValuePair<string, string>> recordData)
        {
            var finalMappingFieldDefs = new List<MappingFieldDefinition>();
            foreach (var lookupFieldDefinition in lookupFieldDefinitions)
            {
                var queryStringParameterValues = lookupFieldDefinition.Source.GetFieldsFromBraces();
                var routeParameterValues = lookupFieldDefinition.EndPoint.
                                            GetUrlValueWithNoQueryStrings().
                                            GetFieldsFromBraces();

                var recordKeys = recordData.Select(t => t.Key).ToList().Distinct();
                if (!queryStringParameterValues.ExistIn(recordKeys)) continue;
                if(!routeParameterValues.ExistIn(recordKeys)) continue;
                finalMappingFieldDefs.Add(lookupFieldDefinition);
                _log.Debug($"Lookup Order Source: {lookupFieldDefinition.Source}, Dest:{lookupFieldDefinition.Destination}");
                recordData.Add(new KeyValuePair<string, string>(lookupFieldDefinition.Destination.RemoveBraces(), "Data"));
            }
            return finalMappingFieldDefs;
        }


    }
}
