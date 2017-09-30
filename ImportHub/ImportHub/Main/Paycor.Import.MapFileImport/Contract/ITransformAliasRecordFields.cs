using System.Collections.Generic;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface ITransformAliasRecordFields<in TInput>
    {
        IEnumerable<IEnumerable<KeyValuePair<string, string>>> TransformAliasRecordFields(TInput input, 
            IEnumerable<IEnumerable<KeyValuePair<string,string>>> records, string masterSessionId);
    }
}
