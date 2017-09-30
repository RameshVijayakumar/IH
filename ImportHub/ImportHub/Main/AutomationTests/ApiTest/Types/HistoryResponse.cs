using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Paycor.Import.ImportHubTest.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class HistoryResponse
    {
        [JsonProperty("importDate")]
        public DateTime ImportDate { get; set; }

        [JsonProperty("importDateEpoch")]
        public double ImportDateEpoch { get; set; }

        [JsonProperty("importCompletionDate")]
        public DateTime? ImportCompletionDate { get; set; }

        [JsonProperty("importCompletionDateEpoch")]
        public double ImportCompletionDateEpoch { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("clientId")]
        public int ClientId { get; set; }

        [JsonProperty("importType")]
        public string ImportType { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("status")]
        public ImportHistoryStatus Status { get; set; }
 
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("source")]
        private string Source { get; set; }

        [JsonProperty("importedRecordCount")]
        public int? ImportedRecordCount { get; set; }

        [JsonProperty("failedRecordCount")]
        public int? FailedRecordCount { get; set; }

        [JsonProperty("statusSummaryMessage")]
        public string StatusSummaryMessage { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("importHistoryStatus")]
        public ImportHistoryStatus ImportHistoryStatus { get; set; }

        [JsonProperty("summaryErrorMessage")]
        public string SummaryErrorMessage { get; set; }

        public bool Validate()
        {
            List<bool> result = new List<bool>();
            result.Add(string.Compare(StatusSummaryMessage, 
                $"{ImportedRecordCount} records imported, {FailedRecordCount} records failed", 
                StringComparison.OrdinalIgnoreCase) == 0);
            string expectLink = ImportHistoryStatus == ImportHistoryStatus.Completed ? Id : "";
            result.Add(string.Compare(Link, expectLink,StringComparison.OrdinalIgnoreCase) == 0 );
            result.Add(SummaryErrorMessage.IsNotNullOrEqualToExpectation(SummaryErrorMessage));
            result.Add(Source.IsNotNullOrEqualToExpectation(Source));
            return result.All(x => x);
        } 

    }
}
