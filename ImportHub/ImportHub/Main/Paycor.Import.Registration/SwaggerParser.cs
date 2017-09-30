using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.Registration
{
    public class SwaggerParser : ISwaggerParser
    {
        private readonly IMappingFactory _factory;

        public SwaggerParser(IMappingFactory factory)
        {
            Ensure.ThatArgumentIsNotNull(factory, nameof(factory));
            _factory = factory;
            _factory.LoadHandlers();
        }

        public IEnumerable<GeneratedMapping> GetAllApiMappings(string swaggerText)
        {
            var mappings = new List<GeneratedMapping>();
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            };

            var swaggerDefinition = JsonConvert.DeserializeObject<SwaggerDocumentDefinition>(swaggerText, settings);
           
            if (swaggerDefinition.paths == null)
                return mappings;

            var endpointItems = swaggerDefinition.paths;
            var endpointPosts = swaggerDefinition.paths.
                                Where(x => x.Value.post != null && (!x.Value.post.deprecated && x.Value.post.optIn)).ToDictionary(x => x.Key, x => x.Value);

            var operations = new Dictionary<HtmlVerb, Operation>();
            foreach (var eachEndpointPost in endpointPosts)
            {
                var mappingEndpoints = new MappingEndpoints();
                var endpointPost = swaggerDefinition.FormatEndPoint(eachEndpointPost.Key);
                
                operations.Add(HtmlVerb.Post, eachEndpointPost.Value.post);
                GenerateEndpointsAndAddMappingForEndpoints(mappings, swaggerDefinition, endpointItems, operations, eachEndpointPost, mappingEndpoints, endpointPost);
            }

            return mappings;
        }

        private void GenerateEndpointsAndAddMappingForEndpoints(List<GeneratedMapping> mappings, SwaggerDocumentDefinition swaggerDefinition, IDictionary<string, PathItem> endpointItems, Dictionary<HtmlVerb, Operation> operations, KeyValuePair<string, PathItem> eachEndpointPost, MappingEndpoints mappingEndpoints, string endpointPost)
        {
            var hasEndpointPut = false;
            var hasEndpointDelete = false;
            var hasEndpointPatch = false;
            foreach (var endpointItem in endpointItems)
            {
                string endpointPut = null;
                string endpointDelete = null;
                string endpointPatch = null;
                if (!eachEndpointPost.Key.HasEndpointPutAndDelete(endpointItem.Key)) continue;
                if (endpointItem.Value.put != null && endpointItem.Value.put.optIn)
                {
                    hasEndpointPut = true;
                    endpointPut = swaggerDefinition.FormatEndPoint(endpointItem.Key);
                }
                if (endpointItem.Value.delete != null && endpointItem.Value.delete.optIn)
                {
                    hasEndpointDelete = true;
                    endpointDelete = swaggerDefinition.FormatEndPoint(endpointItem.Key);
                }
                if (endpointItem.Value.patch != null && endpointItem.Value.patch.optIn)
                {
                    hasEndpointPatch = true;
                    endpointPatch = swaggerDefinition.FormatEndPoint(endpointItem.Key);
                }
                mappingEndpoints.Post = endpointPost;
                mappingEndpoints.Put = endpointPut;
                mappingEndpoints.Delete = endpointDelete;
                mappingEndpoints.Patch = endpointPatch;
                AddMappingForSelectedEndPoints(operations, mappings, swaggerDefinition, mappingEndpoints);
                operations.Clear();
                break;
            }
            if (hasEndpointPut || hasEndpointDelete || hasEndpointPatch)
            {
                return;
            }
            mappingEndpoints.Post = endpointPost;
            mappingEndpoints.Put = null;
            mappingEndpoints.Delete = null;
            mappingEndpoints.Patch = null;
            AddMappingForSelectedEndPoints(operations, mappings, swaggerDefinition, mappingEndpoints);
            operations.Clear();
        }

        private void AddMappingForSelectedEndPoints(Dictionary<HtmlVerb, Operation> operations, List<GeneratedMapping> mappings, SwaggerDocumentDefinition swaggerDefinition,
                                                    MappingEndpoints mappingEndpoints)
        {
            var operKeyValuePair = operations.FirstOrDefault();
            mappings.AddRange(GetMappings(swaggerDefinition, operKeyValuePair, mappingEndpoints));
        }


        private IEnumerable<GeneratedMapping> GetMappings(SwaggerDocumentDefinition swaggerDocument, KeyValuePair<HtmlVerb, Operation> operKeyValuePair, MappingEndpoints mappingEndpoints)
        {
            var generator = _factory.GetMappingGenerator(operKeyValuePair.Key);
            if (generator == null)
                return new List<GeneratedMapping>();

            var mappings = generator.GetMappings(swaggerDocument, operKeyValuePair.Value, mappingEndpoints);

            return mappings ?? new List<GeneratedMapping>();
        }
    }
}



