using Swashbuckle.Swagger;
using System.Linq;
using System.Web.Http.Description;

namespace Paycor.Import.Registration.Client
{
    public class ApiVersionFilter : IDocumentFilter
    {
        private readonly string _domainRoot;

        public ApiVersionFilter(string domainRoot)
        {
            _domainRoot = domainRoot;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            var privateVersion = swaggerDoc.info.version == "unversioned";
            if (privateVersion == false)
            {
                var @public = swaggerDoc.paths
                    .Where(p => p.Key.StartsWith("/" + _domainRoot + "/" + swaggerDoc.info.version))
                    .ToDictionary(p => p.Key, p => p.Value);

                swaggerDoc.paths = @public;
            }
            else
            {
                var @private = swaggerDoc.paths
                    .Where(p => p.Key.StartsWith("/" + _domainRoot + "/v") == false)
                    .ToDictionary(p => p.Key, p => p.Value);

                swaggerDoc.paths = @private;
            }
        }
    }
}
