using System.Collections.Generic;
using Paycor.Import.Mapping;
// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.FileType
{
    /// <summary>
    /// API Based Algorithm - traverse the set of mappings and rank them based on
    /// the percentage of columns in the uploaded file that match fields in the API:
    /// 
    /// percentage = number_of_matched_columns / number_of_fields_in_api * 100
    /// </summary>
    public class ApiBasedAlgorithm : BaseRelevanceAlgorithm
    {
        public override AlgorithmType AlgorithmType => AlgorithmType.ApiBased;

        protected override int? ComputeRanking(ApiMapping apiMapping, IEnumerable<string> columns)
        {
            return ApiBasedAlgorithm(apiMapping, columns);
        }
    }
}
