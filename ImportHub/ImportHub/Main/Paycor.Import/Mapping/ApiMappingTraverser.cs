using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;
// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.Mapping
{
    public static class ApiMappingTraverser
    {
        public static IEnumerable<ApiMapping> GetBestMatchedMappings(IEnumerable<ApiMapping> mappings,
            IEnumerable<string> columns)
        {
            var results = new List<ApiMapping>();
            if (columns.Contains(string.Empty) || columns.Contains(null))
            {
                var ordinalFields = columns.ToList().ReplaceEmptyValuesWithOrdinals().Where(t => t.IsOrdinal());

                // TODO: Account for partial header maps
                results.AddRange(from map in mappings
                    let ordinalSources =
                    map.Mapping.FieldDefinitions.Where(t => t.Source.IsOrdinal()).Select(t => t.Source)
                    where ordinalSources.Any() && ordinalSources.Count() == ordinalFields.Count()
                    select map);
            }

            var matchedMappings = GetMatchedMappingsBasedOnRouteParamAndLookupParam(mappings, columns);
            results.AddRange(matchedMappings);

            // TODO: May want to remove distict if we can determine this can be addressed by
            // comment below.
            return results.Distinct();
        }

        private static IEnumerable<ApiMapping> GetMatchedMappingsBasedOnRouteParamAndLookupParam(
            IEnumerable<ApiMapping> mappings, IEnumerable<string> columns)
        {
            var cleanColumns = CleanColumns(columns);
            var results = new List<ApiMapping>();
            foreach (var mapping in mappings)
            {
                // TODO: Fixit - why are we doing this when we account for 
                // headerless by ordinal check above. This leads to the duplication and need to
                // return results.Distinct() above.
                if (mapping.IsHeaderlessCustomMap())
                {
                    results.Add(mapping);
                }

                // Grab the "primary route", which is presumed to be POST in most cases
                // as this will fill the minimum needed route parameters to satisfy the
                // matching checks. If POST is not found then fallback through the other modifying
                // Html verbs (PUT, PATCH, DELETE).
                var routeUrl = GetPrimaryRouteFromMappingEndpoints(mapping.MappingEndpoints);
                var routeParameters = routeUrl.GetFieldsFromBraces()
                    .Select(s => new Column
                    {
                        Name = s,
                        ValueExists = cleanColumns.Contains(s, StringComparer.OrdinalIgnoreCase)
                    }).ToList();

                if (AreAllRouteParametersSatisfied(routeParameters))
                {
                    results.Add(mapping);
                    continue;
                }

                var lookupFields = mapping.Mapping.FieldDefinitions.Where(m => m.EndPoint != null);

                foreach (var parameter in routeParameters.Where(f => f.ValueExists == false))
                {
                    var validLookups =
                        lookupFields.Where(
                            m => string.Equals(m.Destination.RemoveBraces(), parameter.Name,
                                StringComparison.OrdinalIgnoreCase));
                    foreach (var lookup in validLookups)
                    {
                        var lookupParameters = lookup.Source.GetFieldsFromBraces();
                        if (lookupParameters.All(s => cleanColumns.Contains(s, StringComparer.OrdinalIgnoreCase)))
                        {
                            parameter.ValueExists = true;
                            break;
                        }
                    }
                }

                if (AreAllRouteParametersSatisfied(routeParameters))
                {
                    results.Add(mapping);
                }
            }
            return results;
        }

        public static IEnumerable<ApiMapping> GetMappingsThatFullyCoverColumns(IEnumerable<ApiMapping> mappings,
            IEnumerable<string> columns)
        {
            var cleanColumns = CleanColumns(columns);
            foreach (var map in mappings)
            {
                var sources = map.Mapping.FieldDefinitions.CleanAndFlattenSourceFields();

                if (!cleanColumns.Except(sources, StringComparer.OrdinalIgnoreCase).Any())
                {
                    yield return map;
                }
            }
        }

        //TODO: Complete partial tests
        private static string GetPrimaryRouteFromMappingEndpoints(MappingEndpoints endpoints)
        {
            if (!string.IsNullOrEmpty(endpoints.Post))
                return endpoints.Post;
            if (!string.IsNullOrEmpty(endpoints.Patch))
                return endpoints.Patch;
            if (!string.IsNullOrEmpty(endpoints.Put))
                return endpoints.Put;
            if (!string.IsNullOrEmpty(endpoints.Delete))
                return endpoints.Delete;

            return null;
        }

        private static bool AreAllRouteParametersSatisfied(IEnumerable<Column> routeParameters)
        {
            return routeParameters.All(f => f.ValueExists);
        }

        private class Column
        {
            public string Name { get; set; }
            public bool ValueExists { get; set; }
        }

        private static IEnumerable<string> CleanColumns(IEnumerable<string> columns)
        {
            var cleanColums = columns.Where(f => !string.IsNullOrEmpty(f))
                .Select(f => f.RemoveBraces().RemoveWhiteSpaces().ToLower());

            return cleanColums;
        }
    }
}
