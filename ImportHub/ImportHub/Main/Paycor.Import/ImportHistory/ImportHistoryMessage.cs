using System;
using Newtonsoft.Json;

//TODO: Missing unit tests

namespace Paycor.Import.ImportHistory
{
    public class ImportHistoryMessage : RepositoryObject
    {
        public DateTime ImportDate { get; set; }

        public double ImportDateEpoch { get; set; }

        public DateTime? ImportCompletionDate { get; set; }

        public double ImportCompletionDateEpoch { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string TransactionId { get; set; }

        public string ClientId { get; set; }

        public string UserName { get; set; }

        public string ImportType { get; set; }

        public string FileType { get; set; }

        public string FileName { get; set; }

        public string Status => ImportHistoryStatus.ToString();

        public string User { get; set; }

        private string _source;

        public string Source
        {
            get { return string.IsNullOrWhiteSpace(_source) ? User : _source; }
            set { _source = value; }
        }

        public int? ImportedRecordCount { get; set; }

        public int? FailedRecordCount { get; set; }

        public int? CancelledRecordCount { get; set; }

        public string IsMarkedForDelete { get; set; }

        public string StatusSummaryMessage => ImportHistoryStatus == ImportHistoryStatusEnum.Completed
            ? $"{ImportedRecordCount ?? 0} records imported, {FailedRecordCount ?? 0} records failed, {CancelledRecordCount ?? 0} records cancelled"
            : ImportHistoryStatus.ToString();

        public string Link => ImportHistoryStatus == ImportHistoryStatusEnum.Completed ? TransactionId : "";

        public ImportHistoryStatusEnum ImportHistoryStatus { get; set; }

        private string _summaryErrorMessage;

        public string SummaryErrorMessage
        {
            get { return string.IsNullOrEmpty(_summaryErrorMessage) ? "" : _summaryErrorMessage; }

            set { _summaryErrorMessage = value; }
        }
    }
}
