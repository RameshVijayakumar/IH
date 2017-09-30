using System.Collections.Generic;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IFlatToApiRecordsTransformer
    {
        IList<ApiRecord> TranslateFlatRecordsToApiRecords(IEnumerable<IDictionary<string, string>> records, ApiMapping mapping, ImportContext context);
    }
}