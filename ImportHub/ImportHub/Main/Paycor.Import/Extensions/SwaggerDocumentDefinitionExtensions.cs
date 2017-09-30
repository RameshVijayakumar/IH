using System;
using System.Linq;
using Paycor.Import.Mapping;
//TODO: Incomplete unit test
namespace Paycor.Import.Extensions
{
    public static class SwaggerDocumentDefinitionExtensions
    {
        public static string FormatEndPoint(this SwaggerDocumentDefinition swaggerDocumentDefinition, string endPoint)
        {
            var scheme = swaggerDocumentDefinition.schemes.Any(s => s.Equals("https", StringComparison.OrdinalIgnoreCase)) ? "https" : swaggerDocumentDefinition.schemes.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new Exception("Unable to Determine if the Endpoint is Http or Https");
            }
            return scheme + "://" + swaggerDocumentDefinition.host + swaggerDocumentDefinition.basePath + endPoint;
        }

        public static string FormatEndPointIgnoringBasePath(this SwaggerDocumentDefinition swaggerDocumentDefinition, string endPoint)
        {
            var scheme = swaggerDocumentDefinition.schemes.Any(s => s.Equals("https", StringComparison.OrdinalIgnoreCase)) ? "https" : swaggerDocumentDefinition.schemes.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new Exception("Unable to Determine if the Endpoint is Http or Https");
            }
            return scheme + "://" + swaggerDocumentDefinition.host + endPoint;
        }

        public static string GetMapTypeNameForOperation(this SwaggerDocumentDefinition swaggerDocumentDefinition,
            Operation operation)
        {
            if (operation != null && operation.parameters == null)
                return null;

            var schema = operation?.parameters.Where(t => t.schema != null).Select(t => t.schema).FirstOrDefault();
            if (schema == null)
                return null;

            var definitionSchema = schema.GetSchemaFromRef(swaggerDocumentDefinition);
            return definitionSchema?.mapType;
        }
    }
}
