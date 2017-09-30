using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Messaging;

namespace Paycor.Import.Mapping
{
    public abstract class BaseMappingGeneratorFactory : IMappingFactory
    {
        private readonly List<IMappingGenerator> _loadedGenerators = new List<IMappingGenerator>();
        public abstract void LoadHandlers();
        protected void AddMappingGenerators(IEnumerable<IMappingGenerator> generators)
        {
            _loadedGenerators.AddRange(generators);
        }

        public IMappingGenerator GetMappingGenerator(HtmlVerb verb)
        {
            return _loadedGenerators.FirstOrDefault(loaded => loaded.Verb == verb);
        }
    }
}
