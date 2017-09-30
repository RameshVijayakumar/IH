using System.Collections.Generic;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IRecordSplitter<in TInput>
    {
        IEnumerable<Dictionary<string, string>> TransformRecordsToDictionaryList(TInput input, IEnumerable<IEnumerable<KeyValuePair<string, string>>> records);
    }
}
