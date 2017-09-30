using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Paycor.Import.Registration.Client
{
    public class BatchChunkingSupportFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var attribute = apiDescription.ActionDescriptor
                .GetCustomAttributes<BatchChunkingSupportAttribute>()
                .FirstOrDefault();
            if (attribute == null) return;
            
            operation.vendorExtensions.Add("x-batchChunkingSupported", true);
            operation.vendorExtensions.Add("x-preferredBatchChunkSize", attribute.PreferredChunkSize);
        }

    }

}
