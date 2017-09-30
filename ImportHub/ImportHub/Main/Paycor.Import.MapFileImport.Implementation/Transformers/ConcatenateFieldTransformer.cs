using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;
using Paycor.Import.Validator;

// ReSharper disable AssignNullToNotNullAttribute

namespace Paycor.Import.MapFileImport.Implementation.Transformers
{
    public class ConcatenateFieldTransformer : NullMappingDefinitionValidator, ITransformRecordFields<MappingDefinition>
    {
        public IEnumerable<KeyValuePair<string, string>> TransformRecordFields(MappingDefinition mappingDefinition, string masterSessionId,
            IDictionary<string, string> record = null, IEnumerable<KeyValuePair<string, string>> recordKeyValuePairs = null,
            ILookup lookup = null)
        {
            string errorMessage;

            if (!Validate(mappingDefinition, out errorMessage))
                return new List<KeyValuePair<string, string>>();

            if (recordKeyValuePairs == null) return null;
            var recordData = recordKeyValuePairs.ToList();
            var modifiedItems = (from mappingFieldDefinition in mappingDefinition.FieldDefinitions
                where string.IsNullOrWhiteSpace(mappingFieldDefinition.EndPoint)
                where mappingFieldDefinition.Source.IsConcatenation()
                where mappingFieldDefinition.Source != null
                let concatenationFields = mappingDefinition.GetConcatenationFields(mappingFieldDefinition)
                let concatenatedData = recordData.ConcatenateRecordFields(concatenationFields)
                select GetDestinationItem(mappingFieldDefinition, concatenatedData)
            ).ToList();

            recordData.AddOrOverwriteKeyValuePair(modifiedItems);

            RemoveDataNotNeededForPayload(mappingDefinition, modifiedItems, recordData);
            return recordData;
        }

        private static void RemoveDataNotNeededForPayload(MappingDefinition mappingDefinition, 
            IEnumerable<KeyValuePair<string, string>> modifiedItems,
            List<KeyValuePair<string, string>> recordData)
        {
            foreach (var modifiedItem in modifiedItems)
            {
                var sources = mappingDefinition.GetAllSourcesUsedForConcat(modifiedItem.Key);
                foreach (var source in sources)
                {
                    if (!mappingDefinition.IsFieldDefinitionExists(source))
                    {
                        recordData.RemoveAll(t => t.Key == source);
                    }
                }
            }
        }

        private static KeyValuePair<string, string> GetDestinationItem(MappingFieldDefinition mappingFieldDefinition, string concatenatedData)
        {
            var item = new KeyValuePair<string, string>();
            if (mappingFieldDefinition.Source == null && mappingFieldDefinition.Destination != null)
            {
                item = new KeyValuePair<string, string>(mappingFieldDefinition.Destination, null);
            }
            if (mappingFieldDefinition.Destination != null)
            {
                item = new KeyValuePair<string, string>(mappingFieldDefinition.Destination, concatenatedData);
            }
            return item;
        }
    }
}