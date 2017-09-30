using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Mapping;
// ReSharper disable All

namespace Paycor.Import.Extensions
{
    public static class ApiMappingExtensions
    {
        public static bool IsHeaderlessCustomMap(this ApiMapping mapping)
        {
            var commonSavedMapping = mapping as CommonSavedMapping;

            if (commonSavedMapping == null)
                return false;

            return commonSavedMapping.HasHeader ?? true;
        }

        public static IList<string> GetAllRequiredFields(this ApiMapping mapping)
        {
            return mapping?.Mapping?.FieldDefinitions?.Where(t => t.Required && !t.Source.ContainsOpenAndClosedBraces()).Select(t => t.Source).ToList();
        }

        public static IList<string> GetLookupEndpointsWithoutQueryString(this ApiMapping mapping)
        {
            return 
                mapping?.Mapping?.FieldDefinitions.Where(t => t.Source.ContainsOpenAndClosedBraces())
                    .Select(t => t.EndPoint.Substring(0, t.EndPoint.IndexOf("?", StringComparison.Ordinal))).ToList();
        }

        public static bool IsEndPointOtherThanPost(this ApiMapping mapping)
        {
            return !string.IsNullOrWhiteSpace(mapping?.MappingEndpoints.Delete)
                   ||
                   !string.IsNullOrWhiteSpace(mapping?.MappingEndpoints.Patch)
                   ||
                   !string.IsNullOrWhiteSpace(mapping?.MappingEndpoints.Put);
        }

        public static bool IsMappingSourceEmptyOrNull(this ApiMapping mapping)
        {
            if (mapping?.Mapping?.FieldDefinitions == null) return true;

            var fieldDef = mapping.Mapping.FieldDefinitions.Where(t => t.Source.RemoveWhiteSpaces() == null || t.Source.RemoveWhiteSpaces() == string.Empty).Select(t => t);
            return fieldDef.Any();
        }

        public static void UpdateGeneratedMappingName(this IEnumerable<ApiMapping> mappings)
        {
            if (mappings != null)
            {
                foreach (var mapping in mappings)
                {
                    if (mapping != null) mapping.UpdateGeneratedMappingName();
                }
            }
        }

        public static void UpdateGeneratedMappingName(this ApiMapping mapping)
        {
            if (mapping != null)
                mapping.GeneratedMappingName = mapping.MappingName;
        }

        public static IList<string> GetAllDestRequiredFields(this ApiMapping mapping)
        {
            return mapping?.Mapping?.FieldDefinitions?.Where(t => t.Required && !t.Source.ContainsOpenAndClosedBraces()).Select(t => t.Destination).ToList();
        }
    }
}
