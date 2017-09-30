using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Mapping;

namespace Paycor.Import.Extensions
{
    public static class MappingDefinitionExtensions
    {
        public static IEnumerable<MappingFieldDefinition> GetAllLookupFieldDefinitions(
            this MappingDefinition mappingDefinition,
            IEnumerable<KeyValuePair<string, string>> record,
            IEqualityComparer<MappingFieldDefinition> comparer)
        {
            if (record == null) return null;
            var keys = record.Select(t => t.Key).Distinct().ToList();
            var mappingFieldDefinitions = new List<MappingFieldDefinition>();
            var lookupDestinations = new List<string>();

            foreach (var key in keys)
            {
                var lookup = mappingDefinition.FieldDefinitions.GetFieldDefinitionWithLookUp(key).ToList();
                mappingFieldDefinitions.AddRange(lookup);
                lookupDestinations.AddRange(lookup.Select(l => l.Destination.RemoveBraces()));
            }

            foreach (var lookupDestination in lookupDestinations)
            {
                var lookup = mappingDefinition.FieldDefinitions.GetFieldDefinitionWithLookUp(lookupDestination).ToList();
                mappingFieldDefinitions.AddRange(lookup);
            }
            return mappingFieldDefinitions.Distinct(comparer).ToList();
        }

        public static IEnumerable<string> GetAllSourceFieldsWithoutLookupAndConst(
            this MappingDefinition mappingDefinition)
        {
            return mappingDefinition.FieldDefinitions.Where(
                    t => !t.Source.ContainsOpenAndClosedBraces() && t.SourceType != SourceTypeEnum.Const)
                .Select(t => t.Source);
        }

        public static IEnumerable<string> GetAllDestinationFieldsWhichAreLookups(
            this MappingDefinition mappingDefinition)
        {
            return mappingDefinition.FieldDefinitions.Where(t => t.Destination.ContainsOpenAndClosedBraces()).Select(
                t => t.Destination.RemoveBraces());
        }

        public static IEnumerable<string> GetAllSourceFieldsWithGenericSubArray(
            this MappingDefinition mappingDefinition)
        {
            return mappingDefinition.FieldDefinitions.Where(
                    f => (f.Destination.Replace(" ", string.Empty).Contains("[]")))
                .Select(m => m.Source);
        }

        public static IEnumerable<string> GetHeaderFieldsFromMappingFields(
            this MappingDefinition mappingDefinition)
        {
            return mappingDefinition.GetAllSourceFieldsWithoutLookupAndConst()
                .Except(mappingDefinition.GetAllSourceFieldsWithGenericSubArray())
                .Except(
                    mappingDefinition.GetAllDestinationFieldsWhichAreLookups(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static IEnumerable<string> GetHeaderFieldsFromMappingFieldsWithActionOnTop(
            this MappingDefinition mappingDefinition)
        {
            return mappingDefinition.
                GetHeaderFieldsFromMappingFields()
                .OrderByDescending(t => t == ImportConstants.ActionColumnName);
        }

        public static string GetDestinationField(this MappingDefinition mappingDefinition, string source)
        {
            return source == null ? null : mappingDefinition.FieldDefinitions.Where(t => t.Source != null && t.Source.RemoveWhiteSpaces().ToLower() == source.RemoveWhiteSpaces().ToLower()).Select(t => t.Destination).FirstOrDefault();
        }

        public static IEnumerable<string> GetAllSourcesUsedForConcat(this MappingDefinition mappingDefinition, string destination)
        {
            var data = mappingDefinition.FieldDefinitions.Where(t => t.Destination == destination)
                .Select(t => t.Source.ExceptFirstValueAfterPipe()).FirstOrDefault();
            return data;
        }

        public static bool IsFieldDefinitionExists(this MappingDefinition mappingDefinition, string source)
        {
            return mappingDefinition.FieldDefinitions.Any(t => t.Source == source);
        }

        public static bool IsSourceUsedforConcatenation(this MappingDefinition mappingDefinition,
            string source)
        {
            if (source == null) return false;
            foreach (var fieldDefinition in mappingDefinition.FieldDefinitions)
            {
                if (fieldDefinition.Source.IsConcatenation() && fieldDefinition.Source.Contains(source))
                    return true;
            }
            return false;
        }

        public static IEnumerable<MappingFieldDefinition> GetAllHeaderAsFieldDefinitions(
            this MappingDefinition mappingDefinition)
        {
            return mappingDefinition.FieldDefinitions.Where(x => x.SourceType == SourceTypeEnum.HeaderAsValue);
        }

        public static IEnumerable<string> GetAllLookupSourceFields(this MappingDefinition mappingDefinition)
        {
            var sourceFields = new List<string>();
            var sourceWithBraces =
                mappingDefinition.FieldDefinitions.Where(t => t.Source.ContainsOpenAndClosedBraces())
                    .Select(t => t.Source);

            foreach (var source in sourceWithBraces)
            {
                sourceFields.AddRange(source.GetFieldsFromBraces().Select(t => t.RemoveBraces()));
            }

            return sourceFields.Distinct();
        }

        public static IEnumerable<string> GetAllDestinationFields(this MappingDefinition mappingDefinition)
        {
            return mappingDefinition.FieldDefinitions.Select(t => t.Destination);
        }

        private static IEnumerable<MappingFieldDefinition> GetFieldDefinitionWithLookUp(
            this IEnumerable<MappingFieldDefinition> mappingFieldDefinitions, string recordKey)
        {
            return
                mappingFieldDefinitions.Where(
                        t =>
                            (t.Source != null) &&
                            (t.Source.ToLower().Trim() == recordKey.AddBraces().ToLower().Trim() ||
                             t.Source.ToLower().Trim().Contains(recordKey.AddBraces().ToLower().Trim())))
                    .Select(t => t).ToList();
        }

    }
}
