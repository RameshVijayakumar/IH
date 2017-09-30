using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.Http;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IGenerateFailedRecord
    {
        FailedRecord GetFailedRecord(ApiRecord apiRecord, ErrorResponse errorResponseData,
            HttpExporterResult result);

    }
}
