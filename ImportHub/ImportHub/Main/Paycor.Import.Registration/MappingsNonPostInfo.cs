using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public class MappingsNonPostInfo : IMappingsNonPostInfo
    {
        private readonly ILog _log;

        public MappingsNonPostInfo(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
        }

        public void LogAllNonOptInPostMapNames(string swaggerText)
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            };

            var swaggerDefinition = JsonConvert.DeserializeObject<SwaggerDocumentDefinition>(swaggerText, settings);
            var nonOptInPosts = swaggerDefinition.paths.
                                Where(x => x.Value.post != null && !x.Value.post.deprecated && !x.Value.post.optIn).ToDictionary(x => x.Key, x => x.Value);
            if (!nonOptInPosts.Any()) return;
            var mapNames = GetMappingNameWithNonOptInPost(nonOptInPosts, swaggerDefinition);
            var mappingNames = mapNames.ToList().ConcatListOfString(",");
            _log.Warn($"Mappings {mappingNames} cannot be registered as it has no post opted in");
        }
        
        private static IList<string> GetMappingNameWithNonOptInPost(Dictionary<string, PathItem> nonOptInPosts,
            SwaggerDocumentDefinition swaggerDefinition)
        {
            return nonOptInPosts.Select(nonOptInPost => swaggerDefinition.GetMapTypeNameForOperation(nonOptInPost.Value.post)).ToList();
        }

        
    }
}
