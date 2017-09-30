using System.Collections.Generic;
using Paycor.Import.Messaging;

namespace Paycor.Import.Mapping
{
    public interface IMappingGenerator
    {
        HtmlVerb Verb { get; }
        IEnumerable<GeneratedMapping> GetMappings(SwaggerDocumentDefinition swaggerDocument, Operation operation, MappingEndpoints mappingEndpoints);
    }
}
