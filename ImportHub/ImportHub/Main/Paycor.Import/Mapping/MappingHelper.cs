using System.Collections.Generic;

namespace Paycor.Import.Mapping
{
    public static class MappingHelper
    {
        public static IDictionary<string, string> TransformRecord(IDictionary<string, string> record,
                                                                  MappingDefinition mappingDefinition,
                                                                  ITransformRecord<MappingDefinition> recordTransformer,
                                                                  string masterSessionId = null
            )
        {
            if (mappingDefinition == null || mappingDefinition.FieldDefinitions == null)
            {
                return record;
            }

            IDictionary<string, string> result = new Dictionary<string, string>();

            if (record == null)
            {
                return result;
            }

            if (recordTransformer != null)
                return recordTransformer.TransformRecord(mappingDefinition, record, masterSessionId);

            foreach (var definition in mappingDefinition.FieldDefinitions)
            {
                var valueOfField = SourceTypeHandlerFactory.HandleSourceType(definition).Resolve(definition, definition.Source, record);
                if (definition.Destination != null)
                    result[definition.Destination] = valueOfField;
            }
            return result;
        }
    }

}
