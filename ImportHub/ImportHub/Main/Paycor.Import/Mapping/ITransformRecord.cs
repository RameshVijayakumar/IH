using System.Collections.Generic;

namespace Paycor.Import.Mapping
{
    public interface ITransformRecord<in TInput> 
    {
        IDictionary<string, string> TransformRecord(TInput input, IDictionary<string, string> record, string masterSessionId);
    }
}