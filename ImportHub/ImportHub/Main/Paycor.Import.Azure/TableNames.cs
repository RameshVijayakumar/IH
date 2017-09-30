namespace Paycor.Import.Azure
{
    public static class TableNames
    {
        public const string Cancellations = "Cancellations";
    }

    public static class PartitionKeys
    {
        public const string CancelTokenKey = "CancelTokenPartitionKey";
    }
}
