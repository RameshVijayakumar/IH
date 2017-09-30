using System.Collections.Generic;
using Paycor.Import.Mapping;
// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.FileType
{
    /// <summary>
    /// File and API Based Algorithm - traverse the set of mappings and rank them
    /// based on the average of the API based and File based algorithms:
    /// 
    /// percentage = ((number_of_matched_columns / number_of_fields_in_api * 100) +
    ///              (number_of_matched_fields / number_of_columns_in_file * 100)) / 2
    /// </summary>
    public class FileAndApiBasedAlgorithm : BaseRelevanceAlgorithm
    {
        public override AlgorithmType AlgorithmType => AlgorithmType.FileAndApiBased;

        protected override int? ComputeRanking(ApiMapping apiMapping, IEnumerable<string> columns)
        {
            float p1 = FileBasedAlgorithm(apiMapping, columns);
            float p2 = ApiBasedAlgorithm(apiMapping, columns);
            var percentage = (int) ((p1 + p2)/2.0);
            return percentage;
        }
    }
}
