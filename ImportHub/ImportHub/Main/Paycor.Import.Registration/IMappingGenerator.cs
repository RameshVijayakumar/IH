using System.Collections.Generic;
using Paycor.Import.Mapping;


namespace Paycor.Import.Registration
{
    public interface IMappingGenerator
    {
        HtmlVerb Verb { get; }
        IEnumerable<ApiMapping> GetMappings(SwaggerDocumentDefinition swaggerDocument, Operation operation, string endPoint);
    }
}