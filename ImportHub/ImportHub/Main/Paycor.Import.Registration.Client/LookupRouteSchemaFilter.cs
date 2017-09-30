using Swashbuckle.Swagger;
using System;
using System.Linq;

namespace Paycor.Import.Registration.Client
{
    public class LookupRouteSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(LookupRouteAttribute), true);
            if (attributes.Length > 0)
            {
                var lookups = attributes.Cast<LookupRouteAttribute>()
                    .Select(l => new { l.Route, l.Property, l.ExceptionMessage, l.ValuePath, l.IsRequiredForPayload })
                    .ToDictionary(l => l.Route, l => new { l.ExceptionMessage, l.Property, l.ValuePath, l.IsRequiredForPayload });

                schema.vendorExtensions.Add("x-lookupRoutes", lookups);
            }
        }
    }
}