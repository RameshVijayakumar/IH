using System.Collections.Generic;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public interface IPayloadExtracter
    {
        List<PayloadData> PreparePayload(ImportContext context, IEnumerable<ApiRecord> apiRecords);
        List<ErrorResultData> GetErrorResultItems();

        PreparePayloadTypeEnum GetPreparePayloadType();
    }
}