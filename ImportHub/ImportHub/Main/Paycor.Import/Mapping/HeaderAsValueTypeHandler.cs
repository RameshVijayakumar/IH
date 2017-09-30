using System.Collections.Generic;

namespace Paycor.Import.Mapping
{
    public class HeaderAsValueTypeHandler : FileSourceTypeHandler
    {
        public override string Resolve(MappingFieldDefinition mappingFieldDefinition, string field, IDictionary<string, string> record)
        {
            int testInt;
            if (string.IsNullOrWhiteSpace(field) || int.TryParse(field, out testInt))
            {
                return null;
            }
            var value = base.Resolve(mappingFieldDefinition, field, record);
            return value;
        }
    }
}
