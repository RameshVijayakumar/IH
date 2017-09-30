using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class Map
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "mappingName")]
        public string MappingName { get; set; }

        [JsonProperty(PropertyName = "mappingEndpoints")]
        public MappingEndpoints MappingEndpoints { get; set; }

        [JsonProperty(PropertyName = "mapping")]
        public FieldDefinitionCollection Mapping { get; set; }

        [JsonProperty(PropertyName = "docUrl")]
        public string DocUrl { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "isMappingValid")]
        public bool IsMappingValid { get; set; }

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

        public override string ToString() => this.Serialize();

        public bool Validate()
        {
            List<bool> result = new List<bool>();
            result.Add(ValidateId());
            result.Add(MappingEndpoints.Validate());
            result.Add(ValidateMappingName());
            result.Add(Mapping.Validate());
            result.Add(ValidateObjectType());
            result.Add(ValidateSystemType());
            return result.All(x => x);
        }

        bool ValidateId(string id = null)
        {
           return Id.IsNotNullOrEqualToExpectation(id);
        }

        bool ValidateMappingName(string mappingName = null)
        {
            return Id.IsNotNullOrEqualToExpectation(mappingName);
        }

        bool ValidateObjectType(string objectType = null)
        {
            return Id.IsNotNullOrEqualToExpectation(objectType);
        }

        bool ValidateSystemType(string systemType = null)
        {
            return Id.IsNotNullOrEqualToExpectation(systemType);
        }

    }
}
