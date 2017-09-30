using Paycor.Import.Messaging;

namespace Paycor.Import.Mapping
{
    public interface IMappingFactory
    {
        void LoadHandlers();
        IMappingGenerator GetMappingGenerator(HtmlVerb verb);
    }
}