using System.Collections.Generic;

namespace Paycor.Import.Mapping
{
    public class ConstantSourceTypeHandler : ISourceTypeHandler
    {
        public string Resolve(MappingFieldDefinition mappingFieldDefinition, string field, IDictionary<string, string> record)
        {
            return mappingFieldDefinition?.Source;
        }
    }
}
