using System;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Paycor.Import.Service.Shared
{
    public class FilesPathDocFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            const string refKey = "/importhub/v1/files_doc_ref_only";
            const string fileKey = "/importhub/v1/files";
            var reference = swaggerDoc.paths[refKey];
            swaggerDoc.paths[fileKey].post.parameters = reference.post.parameters;
            swaggerDoc.paths.Remove(refKey);
        }
    }
}