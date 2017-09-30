using Swashbuckle.Swagger;
using System.Linq;
using System.Web.Http.Description;

namespace Paycor.Import.Registration.Client
{
    public class OptInFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var optInValue = apiDescription.ActionDescriptor
                                .GetCustomAttributes<OptInAttribute>()
                                .Select(a => a.OptIn)
                                .FirstOrDefault();

            operation.vendorExtensions.Add("x-optIn", optInValue);
        }
    }
}