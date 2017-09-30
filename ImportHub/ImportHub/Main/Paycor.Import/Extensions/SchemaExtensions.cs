using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paycor.Import.Mapping;

namespace Paycor.Import.Extensions
{
    public static class SchemaExtensions
    {
        public static Schema GetSchemaFromRef(this Schema schema, SwaggerDocumentDefinition swaggerDocument)
        {
            string definitionName = null;

            if (schema?.@ref != null) definitionName = schema.@ref;
            else if (schema != null && schema.type == "array")
            {
                definitionName = schema.items.@ref;
            }

            Schema definitionSchema;

            if (definitionName == null || !swaggerDocument.definitions.TryGetValue(
                definitionName.Substring(ImportConstants.DefinitionPattern.Length), out definitionSchema)) return null;
            return definitionSchema;
        }
    }
}
