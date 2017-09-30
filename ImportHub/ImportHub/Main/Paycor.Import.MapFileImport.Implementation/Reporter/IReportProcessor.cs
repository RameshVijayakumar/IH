using System.Collections.Generic;
using System.Threading.Tasks;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.Status;

namespace Paycor.Import.MapFileImport.Implementation.Reporter
{
    public interface IReportProcessor
    {
        void SendEvent(MappedFileImportStatusMessage statusMessage);

        void SaveFailedData(string transactionId, List<FailedRecord> failedRecords, int clientId = 0);

        Task SaveStatusFromApiAsync(MappedFileImportStatusMessage statusMessage, MappedFileImportStatusLogger statusLogger,
            IEnumerable<ErrorResultData> errorResultDataItems);


        Task UpdateStatusRecordCountAsync(MappedFileImportStatusMessage statusMessage, MappedFileImportStatusLogger statusLogger,
            int failedRecords, int successRecords, int canceledRecords = 0);

        void SaveFailedData(string transactionId, IDictionary<string, IList<FailedRecord>> failedRecords, int clientId = 0);
    }
}
