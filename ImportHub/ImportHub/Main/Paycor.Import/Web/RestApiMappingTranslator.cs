using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Adapter;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.Web
{
    public class RestApiMappingTranslator : Translator<FileTranslationData<IDictionary<string, string>>, RestApiPayload>
    {
        public MappingDefinition DefaultMappingDefinition { get; set; }

        public ITransformRecord<MappingDefinition> RecordTransformer { get; set; }

        public RestApiMappingTranslator(MappingDefinition defaultMappingDefinition, ITransformRecord<MappingDefinition> recordTransformer = null)
        {
            DefaultMappingDefinition = defaultMappingDefinition;
            RecordTransformer = recordTransformer;
        }

        protected override RestApiPayload OnTranslate(FileTranslationData<IDictionary<string, string>> input)
        {
            var mappingDefinition = input.MappingDefinition ?? DefaultMappingDefinition;

            var result = new RestApiPayload
            {
                ApiEndpoint = input.ApiEndpoint,
                TransactionId = Guid.NewGuid().ToString(),
                Name = input.Name,
                Records = input.Records.Select(record => MappingHelper.TransformRecord(record, mappingDefinition, RecordTransformer)).ToList()
            };

            return (result);
        }
    }
}
