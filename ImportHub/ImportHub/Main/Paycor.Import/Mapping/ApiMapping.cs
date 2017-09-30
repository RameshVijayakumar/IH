using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{
    [ExcludeFromCodeCoverage]
    public class ApiMapping : RepositoryObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "mappingName")]
        public string MappingName { get; set; }

        [JsonProperty(PropertyName = "generatedMappingName")]
        public string GeneratedMappingName { get; set; }

        [JsonProperty(PropertyName = "mappingEndpoints")]
        public MappingEndpoints MappingEndpoints { get; set; }

        [JsonProperty(PropertyName = "mapping")]
        public MappingDefinition Mapping { get; set; }

        [JsonProperty(PropertyName = "mappingDescription")]        
        public string MappingDescription { get; set; }

        [JsonProperty(PropertyName = "mappingCategory")]
        public string MappingCategory { get; set; }

        [JsonProperty(PropertyName = "isBatchSupported")]
        public bool IsBatchSupported { get; set; }

        [JsonProperty("isBatchChunkingSupported")]
        public bool IsBatchChunkingSupported { get; set; }

        [JsonProperty("preferredBatchChunkSize")]
        public int? PreferredBatchChunkSize { get; set; }

        [JsonProperty(PropertyName = "chunkSize")]
        public int ChunkSize { get; set; }

        [JsonProperty(PropertyName = "objectType")]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = "hasHeader")]
        public bool? HasHeader { get; set; }

        public ApiMapping ShallowCopy()
        {
            var baseMapping = (ApiMapping)MemberwiseClone();
            baseMapping.Mapping = new MappingDefinition();

            return baseMapping;
        }
    }
}
