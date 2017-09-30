using System.Collections.Generic;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface ILookupResolver<in T>
    {
        Lookupvalues RetrieveLookupValue(T mappingFieldDefinition, string masterSessionId, 
            IEnumerable<KeyValuePair<string, string>> record, ILookup lookup);
    }
}