using System.Collections.Generic;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public interface INonBatchPayloadPreparer
    {
        List<PayloadData> PreparePayloadForNonBatch(ImportContext context, IEnumerable<ApiRecord> apiRecords);
        List<ErrorResultData> GetErrorResultItems();
    }
}