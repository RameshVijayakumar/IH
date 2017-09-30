using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Paycor.Import.Registration.Client
{
    public class FieldDescriptionSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            foreach (var field in type.GetFields())
            {
                var descriptionAttribute =
                    field.GetCustomAttribute(typeof(DescriptionAttribute), true) as DescriptionAttribute;

                var jsonPropertyAttribute =
                    field.GetCustomAttribute(typeof(JsonPropertyAttribute), true) as JsonPropertyAttribute;

                PerformApply(field.Name, descriptionAttribute, jsonPropertyAttribute, schema);
            }

            foreach (var property in type.GetProperties())
            {
                var descriptionAttribute =
                    property.GetCustomAttribute(typeof(DescriptionAttribute), true) as DescriptionAttribute;

                var jsonPropertyAttribute =
                    property.GetCustomAttribute(typeof(JsonPropertyAttribute), true) as JsonPropertyAttribute;

                PerformApply(property.Name, descriptionAttribute, jsonPropertyAttribute, schema);
            }
        }

        private void PerformApply(string name, DescriptionAttribute descriptionAttribute, JsonPropertyAttribute jsonPropertyAttribute, Schema schema)
        {
            if (descriptionAttribute == null) return;

            if (!string.IsNullOrEmpty(jsonPropertyAttribute?.PropertyName))
            {
                name = jsonPropertyAttribute.PropertyName;
            }

            name = schema.properties.SingleOrDefault(
                x => x.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Key;

            schema.properties[name].vendorExtensions.Add("x-sourceName", descriptionAttribute.Description);
        }

    }
}