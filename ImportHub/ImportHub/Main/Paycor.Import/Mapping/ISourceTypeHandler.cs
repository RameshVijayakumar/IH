using System.Collections.Generic;

namespace Paycor.Import.Mapping
{
    public interface ISourceTypeHandler
    {
        string Resolve(MappingFieldDefinition mappingFieldDefinition, string field, IDictionary<string, string> record);
    }
}
