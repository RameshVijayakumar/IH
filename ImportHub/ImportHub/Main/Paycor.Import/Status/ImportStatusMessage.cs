namespace Paycor.Import.Status
{
    public class ImportStatusMessage
    {
        public string Id { get; set; }

        public string FileName { get; set; }

        public string Message { get; set; }

        public string ImportType { get; set; }

        public string FileType { get; set; }

        public string ClientId { get; set; }

        public string User { get; set; }

        public string UserName { get; set; }

        public string Source { get; set; }

        public decimal PercentComplete { get; set; }

        public int? SuccessRecordsCount { get; set; }

        public int? FailRecordsCount { get; set; }

        public int? WarnRecordCount { get; set; }

        public int? CancelRecordCount { get; set; }

        public int? Current { get; set; }

        public int? Total { get; set; }

        public string ResultsDownloadLink { get; set; }

        public bool IsImportCancelled { get; set; }
    }
}