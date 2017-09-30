using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.FileType
{
    public abstract class BaseRelevanceAlgorithm : IRelevanceAlgorithm
    {
        public abstract AlgorithmType AlgorithmType { get; }

        public IEnumerable<ApiMapping> GetMappingsFor(IEnumerable<ApiMapping> allMappings, IEnumerable<string> columns,
            FileTypeSortOrder sortOrder,
            out IDictionary<string, int?> mapRankings)
        {
            var matched = ApiMappingTraverser.GetBestMatchedMappings(allMappings, columns);

            // If there are mappings that "fully cover" the set of columns, override the filter
            // and just return the set of exact mappings.
            var exact = ApiMappingTraverser.GetMappingsThatFullyCoverColumns(matched, columns);

            if (exact.Any())
            {
                mapRankings = ComputeRankings(exact, columns);
                return ApplySort(exact, sortOrder, mapRankings);
            }

            mapRankings = ComputeRankings(matched, columns);
            return ApplySort(matched, sortOrder, mapRankings);
        }

        protected abstract int? ComputeRanking(ApiMapping apiMapping, IEnumerable<string> columns);

        protected int FileBasedAlgorithm(ApiMapping apiMapping, IEnumerable<string> columns)
        {
            float columnsCount = columns.Count();
            var sourceFields = apiMapping.Mapping.FieldDefinitions.CleanAndFlattenSourceFields();

            // Get back the list of columns that are not in the file.
            var nonMatched = columns.Except(sourceFields, StringComparer.OrdinalIgnoreCase);

            float nonMatchedCount = nonMatched.Count();

            // Calculate as the percentage of matched fields per columns in file.
            var percentage = (int) ((columnsCount - nonMatchedCount)/columnsCount*100.0);
            return percentage;
        }

        protected int ApiBasedAlgorithm(ApiMapping apiMapping, IEnumerable<string> columns)
        {
            var sourceFields = apiMapping.Mapping.FieldDefinitions.CleanAndFlattenSourceFields();
            var sourceFieldCount = sourceFields.Count();

            // Get back the list of columns that are not in the API.
            var nonMatched = sourceFields.Except(columns, StringComparer.OrdinalIgnoreCase);

            float nonMatchedCount = nonMatched.Count();

            // Calculate as the percentage of matched fields per columns in file.
            var percentage = (int) ((sourceFieldCount - nonMatchedCount)/sourceFieldCount*100.0);
            return percentage;
        }

        private IDictionary<string, int?> ComputeRankings(IEnumerable<ApiMapping> matched, IEnumerable<string> columns)
        {
            var rankings = new Dictionary<string, int?>();

            foreach (var apiMapping in matched)
            {
                var ranking = ComputeRanking(apiMapping, columns);
                if (ranking >= 1) rankings[apiMapping.Id] = ranking;
            }

            return rankings;
        }

        private IEnumerable<ApiMapping> ApplySort(IEnumerable<ApiMapping> inMappings, FileTypeSortOrder sortOrder,
            IDictionary<string, int?> rankings)
        {
            if (sortOrder == FileTypeSortOrder.Alpha)
            {
                return inMappings.OrderBy(x => x.MappingName);
            }

            var sortedRankings = from rank in rankings
                orderby rank.Value descending
                select rank;

            var ordered = sortedRankings.Select(kvp => kvp.Key).Select(id => inMappings.First(x => x.Id == id)).ToList();
            return ordered;
        }
    }
}
