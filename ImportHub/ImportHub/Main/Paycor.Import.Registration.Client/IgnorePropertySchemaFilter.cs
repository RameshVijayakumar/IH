using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Paycor.Import.Registration.Client
{
    public class IgnorePropertySchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            foreach (var field in type.GetFields())
            {
                var ignoreAttribute =
                    field.GetCustomAttribute(typeof(IgnorePropertyAttribute), true) as IgnorePropertyAttribute;

                var jsonPropertyAttribute =
                    field.GetCustomAttribute(typeof(JsonPropertyAttribute), true) as JsonPropertyAttribute;

                PerformApply(field.Name, ignoreAttribute, jsonPropertyAttribute, schema);
            }

            foreach (var property in type.GetProperties())
            {
                var ignoreAttribute =
                    property.GetCustomAttribute(typeof(IgnorePropertyAttribute), true) as IgnorePropertyAttribute;

                var jsonPropertyAttribute =
                    property.GetCustomAttribute(typeof(JsonPropertyAttribute), true) as JsonPropertyAttribute;

                PerformApply(property.Name, ignoreAttribute, jsonPropertyAttribute, schema);
            }
        }

        private void PerformApply(string name, IgnorePropertyAttribute ignoreAttribute, JsonPropertyAttribute jsonPropertyAttribute, Schema schema)
        {
            if (ignoreAttribute == null) return;

            if (!string.IsNullOrEmpty(jsonPropertyAttribute?.PropertyName))
            {
                name = jsonPropertyAttribute.PropertyName;
            }

            name = schema.properties.SingleOrDefault(
                x => x.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Key;

            schema.properties[name].vendorExtensions.Add("x-ignore", true);
        }
    }
}
