namespace Paycor.Import.Messaging
{
    public class ImportCancelToken
    {
        public string TransactionId { get; set; }

        public bool CancelRequested { get; set; }
    }
}
