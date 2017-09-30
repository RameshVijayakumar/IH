using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.FileType
{
    public class LegacyAlgorithm : BaseRelevanceAlgorithm
    {
        public override AlgorithmType AlgorithmType => AlgorithmType.Legacy;

        protected override int? ComputeRanking(ApiMapping apiMapping, IEnumerable<string> columns)
        {
            // Legacy algorithm does not compute a ranking.
            return null;
        }
    }
}
