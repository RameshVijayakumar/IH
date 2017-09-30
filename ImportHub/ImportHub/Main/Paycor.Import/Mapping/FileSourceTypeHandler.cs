using System.Collections.Generic;
using Paycor.Import.Extensions;

namespace Paycor.Import.Mapping
{
    public class FileSourceTypeHandler : ISourceTypeHandler
    {
        public virtual string Resolve(MappingFieldDefinition mappingFieldDefinition, string field, IDictionary<string, string> record)
        {
            string result = null;
            if (field != null)
            {
                record?.TryGetValue(field, out result);
            }

            return mappingFieldDefinition.Type != null && 
                mappingFieldDefinition.Type.Trim().ToLower() == "bool" ? result.ConvertToTrueOrFalse() : result;
        }
    }
}
