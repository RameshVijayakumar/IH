using Microsoft.WindowsAzure.Storage.Table;
using Paycor.Import.Messaging;

namespace Paycor.Import.Azure
{
    public class ImportCancelTokenEntity : TableEntity
    {

        public ImportCancelToken AsImportCancelToken()
        {
            return new ImportCancelToken
            {
                CancelRequested = CancelRequested,
                TransactionId = RowKey
            };
        }

        public bool CancelRequested { get; set; }
    }
}
