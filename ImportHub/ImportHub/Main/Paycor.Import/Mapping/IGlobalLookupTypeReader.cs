using System.Collections.Generic;

namespace Paycor.Import.Mapping
{
    public interface IGlobalLookupTypeReader
    {
        GlobalLookupDefinition LookupDefinition(string lookupType);
        IEnumerable<string> GetLookupNames();
    }
}