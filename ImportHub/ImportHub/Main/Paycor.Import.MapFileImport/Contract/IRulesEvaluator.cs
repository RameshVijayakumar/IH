using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IRulesEvaluator
    {
        IEnumerable<MappingFieldDefinition> SortLookupOrder(IEnumerable<KeyValuePair<string, string>> record, MappingDefinition mappingDefinition,
            IEqualityComparer<MappingFieldDefinition> comparer);

        bool ValidateRules(MappingFieldDefinition mappingFieldDefinition, IEnumerable<KeyValuePair<string, string>> previouslyLookedupValues,
            IEnumerable<string> listofRecordKeys
            );
    }
}