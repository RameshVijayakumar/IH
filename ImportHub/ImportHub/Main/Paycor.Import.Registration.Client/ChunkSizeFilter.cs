using System;
using System.Linq;
using Swashbuckle.Swagger;

namespace Paycor.Import.Registration.Client
{
    public class ChunkSizeFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(ChunkSizeAttribute), true);
            if (attributes.Length <= 0)
            {
                return;
            }
            var chunkSize = attributes.Cast<ChunkSizeAttribute>()
                .Select(t => t.ChunkSize).FirstOrDefault();

            schema.vendorExtensions.Add("x-chunkSize", chunkSize);
        }
    }
}
