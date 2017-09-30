using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;
using Paycor.Import.Validator;

namespace Paycor.Import.MapFileImport.Implementation.Transformers
{
    public class SourceFieldTransformer : NullMappingDefinitionValidator, ITransformRecordFields<MappingDefinition>
    {
        public IEnumerable<KeyValuePair<string, string>> TransformRecordFields(MappingDefinition mappingDefinition, string masterSessionId, 
            IDictionary<string, string> record = null, IEnumerable<KeyValuePair<string, string>> recordKeyValuePairs = null,
            ILookup lookup = null)
        {
            var result = new List<KeyValuePair<string, string>>();
            string errorMessage;

            if (!Validate(mappingDefinition, out errorMessage))
                return result;

            foreach (var mappingFieldDefinition in mappingDefinition.FieldDefinitions)
            {
                if (!string.IsNullOrWhiteSpace(mappingFieldDefinition.EndPoint)) continue;

                var sourceList = mappingFieldDefinition.GetAllSources();
                AddItemsToKvp(record, mappingFieldDefinition, sourceList, result, mappingDefinition);
            }

            if (mappingDefinition.FieldDefinitions.Any(t => t.Destination != null && t.Destination == ImportConstants.ActionFieldName))
            {
                result.AddUpsertIfActionisNotPresent();
            }

            return result;
        }

        private static void AddItemsToKvp(IDictionary<string, string> record, MappingFieldDefinition mappingFieldDefinition, IList<string> sources, ICollection<KeyValuePair<string, string>> result,
                                          MappingDefinition mappingDefinition)
        {
            if (mappingFieldDefinition.Source.IsConcatenation())
            {
                AddItemForConcatnatedFields(record, mappingFieldDefinition, sources, result);
            }
            else if (mappingFieldDefinition.HasHeaderAsValue())
            {
                AddItemForHeaderAsValue(record, mappingFieldDefinition, sources, result);
            }
            else
            {
                AddItem(record, mappingFieldDefinition, sources,result,mappingDefinition);
            }
        }

        private static void AddItem(IDictionary<string, string> record, MappingFieldDefinition mappingFieldDefinition, IList<string> sources,
            ICollection<KeyValuePair<string, string>> result, MappingDefinition mappingDefinition)
        {
            foreach (var sourceField in sources)
            {
                var source = sourceField.RemoveWhiteSpaces();
                var fieldValue = SourceTypeHandlerFactory.HandleSourceType(mappingFieldDefinition).Resolve(mappingFieldDefinition,
                source, record);
               
                if (mappingFieldDefinition.Source == null && mappingFieldDefinition.Destination != null)
                {
                    var item = new KeyValuePair<string, string>(mappingFieldDefinition.Destination, null);
                    result.Add(item);
                    continue;
                }

                if (mappingFieldDefinition.Source == null || fieldValue == null)
                {
                    continue;
                }

                if (mappingFieldDefinition.Destination == null) continue;

                if (mappingFieldDefinition.Source.GetFirstValueBeforePipe().RemoveWhiteSpaces() == source)
                {
                    var item = new KeyValuePair<string, string>(mappingFieldDefinition.Destination, fieldValue);
                    result.Add(item);
                    continue;
                }

                var destination = mappingDefinition.GetDestinationField(source);
                if (!string.IsNullOrWhiteSpace(destination) && mappingDefinition.IsSourceUsedforConcatenation(source))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(destination) && mappingDefinition.IsSourceUsedforConcatenation(source))
                {
                    var item = new KeyValuePair<string, string>(source, fieldValue);
                    result.Add(item);
                }

            }
            
        }

        private static void AddItemForHeaderAsValue(IDictionary<string, string> record, MappingFieldDefinition mappingFieldDefinition, IList<string> sources,
            ICollection<KeyValuePair<string, string>> result)

        {
            foreach (var source in sources)
            {
                var sourceField = source.RemoveWhiteSpaces();
                var fieldValue = SourceTypeHandlerFactory.HandleSourceType(mappingFieldDefinition).Resolve(mappingFieldDefinition,
                                    sourceField, record);
                var kvpWithHeaderAsValue = new KeyValuePair<string, string>(mappingFieldDefinition.HeadingDestination, sourceField);
                var kvpWithHeaderValueInDestination = new KeyValuePair<string, string>(mappingFieldDefinition.Destination, fieldValue);
                result.Add(kvpWithHeaderAsValue);
                result.Add(kvpWithHeaderValueInDestination);
            }
        }

        private static void AddItemForConcatnatedFields(IDictionary<string, string> record, MappingFieldDefinition mappingFieldDefinition, IEnumerable<string> sources,
            ICollection<KeyValuePair<string, string>> result)
        {
            var concatenatedData = sources.Select(source => SourceTypeHandlerFactory.HandleSourceType(mappingFieldDefinition)
            .Resolve(mappingFieldDefinition, source.RemoveWhiteSpaces(), record)).Where(fieldValue => !string.IsNullOrWhiteSpace(fieldValue))
            .Aggregate<string, string>(null, (current, fieldValue) => current + fieldValue);

            var item = new KeyValuePair<string, string>();
            if (mappingFieldDefinition.Source == null && mappingFieldDefinition.Destination != null)
            {
                item = new KeyValuePair<string, string>(mappingFieldDefinition.Destination, null);
            }
            if (mappingFieldDefinition.Destination != null)
            {
                item = new KeyValuePair<string, string>(mappingFieldDefinition.Destination, concatenatedData);
            }
            result.Add(item);
        }
    }
}