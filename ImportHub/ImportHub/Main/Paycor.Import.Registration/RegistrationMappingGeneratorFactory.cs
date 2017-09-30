using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public class RegistrationMappingGeneratorFactory : BaseMappingGeneratorFactory
    {
        public override void LoadHandlers()
        {
            AddMappingGenerators(
                new List<IMappingGenerator>
                {
                    new PostMappingGenerator()
                });
        }
    }
}
