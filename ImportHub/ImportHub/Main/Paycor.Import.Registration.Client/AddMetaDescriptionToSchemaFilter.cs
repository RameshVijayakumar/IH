using System;
using System.ComponentModel;
using System.Linq;
using Swashbuckle.Swagger;

namespace Paycor.Import.Registration.Client
{
    public class AddMetaDescriptionToSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            var descriptions = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (descriptions.Any())
            {
                var description = descriptions.Cast<DescriptionAttribute>().First();
                schema.vendorExtensions.Add("x-mapType",
                    string.IsNullOrWhiteSpace(description.Description) ? type.Name : description.Description);
            }
            else
            {
                schema.vendorExtensions.Add("x-mapType", type.Name);
            }

            var categories = type.GetCustomAttributes(typeof(CategoryAttribute), false);
            if (categories.Any())
            {
                var category = categories.Cast<CategoryAttribute>().First();
                schema.vendorExtensions.Add("x-mapCategory", category.Category);
            }
            else
            {
                schema.vendorExtensions.Add("x-mapCategory", "Uncategorized");
            }

            var ihDescriptions = type.GetCustomAttributes(typeof(ImportHubDescriptionAttribute), false);
            if (!ihDescriptions.Any()) return;
            var attribute = ihDescriptions.Cast<ImportHubDescriptionAttribute>().First();
            if (!string.IsNullOrWhiteSpace(attribute.Description))
            {
                schema.vendorExtensions.Add("x-mapDescription", attribute.Description);
            }
        }
    }
}