using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.FileType
{
    public interface IRelevanceAlgorithm
    {
        AlgorithmType AlgorithmType { get; }

        IEnumerable<ApiMapping> GetMappingsFor(IEnumerable<ApiMapping> allMappings, IEnumerable<string> columns,
            FileTypeSortOrder sortOrder, out IDictionary<string, int?> mapRankings);
    }
}
