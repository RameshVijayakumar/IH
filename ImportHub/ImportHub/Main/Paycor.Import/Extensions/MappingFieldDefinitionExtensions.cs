using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Paycor.Import.Mapping;

//TODO: Missing unit tests

namespace Paycor.Import.Extensions
{
    public static class MappingFieldDefinitionExtensions
    {
        public static IEnumerable<string> CleanAndFlattenSourceFields(
            this IEnumerable<MappingFieldDefinition> mappingFieldDefinitions)
        {
            return mappingFieldDefinitions.SelectMany(d =>
            {
                var expandedFields = d.Source.GetFieldsFromBraces();
                return expandedFields.Any()
                    ? expandedFields.Select(f => f.RemoveBraces().RemoveWhiteSpaces().ToLower())
                    : Enumerable.Repeat(d.Source.RemoveBraces().RemoveWhiteSpaces().ToLower(), 1);
            });
        }

        public static void RemoveWhiteSpaceFromFieldDefintionSource(
            this IEnumerable<MappingFieldDefinition> mappingFieldDefinition)
        {
            mappingFieldDefinition.ForEach(t => t.Source = t.Source.RemoveWhiteSpaces());
        }

        public static IEnumerable<MappingFieldDefinition> GetSubArrayFieldsOfMultipleInstances(
            this MappingFieldDefinition mapFieldDefinition, int count, string refKey, string propertyName = null)
        {
            var listOfMappingFieldDefinition = new List<MappingFieldDefinition> {mapFieldDefinition};
            for (var i = 0; i < count; i++)
            {
                var field = new MappingFieldDefinition
                {
                    Destination = propertyName != null ? $"{refKey}[{i}].{propertyName}" : $"{refKey}[{i}]",
                    Source = propertyName != null ? $"{refKey} {propertyName}{i + 1}" : $"{refKey}{i + 1}",
                    Type = mapFieldDefinition.Type,
                    GlobalLookupType = mapFieldDefinition.GlobalLookupType,
                    Required = mapFieldDefinition.Required
                };
                listOfMappingFieldDefinition.Add(field);
            }

            return listOfMappingFieldDefinition;

        }

        public static string GetSourceIgnoringPipe(this MappingFieldDefinition mappingFieldDefinition)
        {
            return mappingFieldDefinition.Source.IsConcatenation()
                  ? mappingFieldDefinition.Source.SplitbyPipe().First()
                  : mappingFieldDefinition.Source;
        }

        public static IEnumerable<string> GetConcatenationFields(this MappingDefinition mappingDefinition,
            MappingFieldDefinition mappingFieldDefinition)
        {
            var destinationFields = new List<string> { mappingFieldDefinition.Destination };
            foreach (var source in mappingFieldDefinition.Source.ExceptFirstValueAfterPipe())
            {
                var fieldDefinition = mappingDefinition.FieldDefinitions.FirstOrDefault(t => t.Source.GetFirstValueBeforePipe() == source);
                destinationFields.Add(fieldDefinition != null ? fieldDefinition.Destination : source);
            }
            return destinationFields;
        }

        public static IList<string> GetAllSources(this MappingFieldDefinition mappingFieldDefinition)
        {
            if(string.IsNullOrWhiteSpace(mappingFieldDefinition.Source)) return new List<string>();

            return mappingFieldDefinition.Source.IsConcatenation()
                  ? mappingFieldDefinition.Source.SplitbyPipe().ToList()
                  : new List<string> { mappingFieldDefinition.Source };
        }

        public static bool HasHeaderAsValue(this MappingFieldDefinition mappingFieldDefinition)
        {
            return mappingFieldDefinition.HeadingDestination != null && mappingFieldDefinition.SourceType == SourceTypeEnum.HeaderAsValue;
        }
    }
}
