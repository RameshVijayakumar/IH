using System.Collections.Generic;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface ITransformRecordFields<in TInput>
    {
        IEnumerable<KeyValuePair<string, string>> TransformRecordFields(TInput input, string masterSessionId, 
            IDictionary<string, string> record = null, IEnumerable<KeyValuePair<string,string>> recordKeyValuePairs=null,
            ILookup lookup=null);
    }
}
