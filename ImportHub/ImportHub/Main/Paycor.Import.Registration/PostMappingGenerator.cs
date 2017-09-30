using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.Registration
{
    public class PostMappingGenerator : BaseMappingGenerator, IMappingGenerator
    {
        public HtmlVerb Verb
        {
            get
            {
                return HtmlVerb.Post;
            }
        }

        protected override bool AreFieldsNeededFromOperation()
        {
            return false;
        }

        protected override bool AreFieldsNeededFromDefinitionSchema()
        {
            return true;
        }
    }
}