using System.Collections.Generic;
using System.Linq;

namespace Paycor.Import.FileType
{
    public class RelevanceAlgorithmFactory
    {
        private readonly IList<IRelevanceAlgorithm> _loaded = new List<IRelevanceAlgorithm>();

        public void LoadHandlers()
        {
            _loaded.Add(new LegacyAlgorithm());
            _loaded.Add(new ApiBasedAlgorithm());
            _loaded.Add(new FileBasedAlgorithm());
            _loaded.Add(new FileAndApiBasedAlgorithm());
        }

        public IRelevanceAlgorithm GetAlgorithm(AlgorithmType algorithmType)
        {
            return _loaded.FirstOrDefault(alg => alg.AlgorithmType == algorithmType);
        }
    }
}
