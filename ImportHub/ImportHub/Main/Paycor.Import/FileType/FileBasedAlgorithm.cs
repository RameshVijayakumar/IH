using System.Collections.Generic;
using Paycor.Import.Mapping;
// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.FileType
{
    /// <summary>
    /// File Based Algorithm - traverse the set of mappings and rank them based on
    /// the percentage of API fields that match columns the columns in the uploaded
    /// file:
    /// 
    /// percentage = number_of_matched_fields / number_of_columns_in_file * 100
    /// </summary>
    public class FileBasedAlgorithm : BaseRelevanceAlgorithm
    {
        public override AlgorithmType AlgorithmType => AlgorithmType.FileBased;

        protected override int? ComputeRanking(ApiMapping apiMapping, IEnumerable<string> columns)
        {
            return FileBasedAlgorithm(apiMapping, columns);
        }
    }
}


