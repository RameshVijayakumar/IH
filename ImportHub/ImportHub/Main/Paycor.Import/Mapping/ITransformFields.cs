using System.Collections.Generic;

namespace Paycor.Import.Mapping
{
    public interface ITransformFields<in TInput>
    {
        IDictionary<string, string> TransformFields(TInput input, IDictionary<string, string> record, string masterSessionId);
    }
}