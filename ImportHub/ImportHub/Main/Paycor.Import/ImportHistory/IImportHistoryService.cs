using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Paycor.Import.ImportHistory
{
    public interface IImportHistoryService
    {
        void SaveImportHistory(ImportHistoryMessage importHistoryMessage, string failedRecordsFile = null);
        Task SaveImportHistoryAsync(ImportHistoryMessage importHistoryMessage, string failedRecordsFile = null);

        IEnumerable<ImportHistoryMessage> GetImportHistoryByUser(string user, int displayCount);

        Task<byte[]> GetFailedRecordsFile(string transactionId);

        Task DeleteImportHistoryByUser(string user);

        Task<bool> DeleteImportHistory(string user, string id);

        ImportHistoryMessage GetImportHistory(string id);

        HttpResponseMessage GetHistoryByDateRange(DateTime? start, DateTime? end);

        void SaveFailedRecords(int clientId, string transactionId, Stream failedRecordsFile);
    }
}
