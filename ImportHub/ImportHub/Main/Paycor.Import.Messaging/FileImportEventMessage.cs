using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Paycor.Import.Messaging
{
    /// <summary>
    /// Message that is placed on the enterprise topic after each file has completed processing by import hub.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FileImportEventMessage
    {
        public string TransactionId { get; set; }

        public string SummaryUrl { get; set; }

        public string SummaryResult { get; set; }

        public long TotalImported { get; set; }

        public long TotalFailed { get; set; }

        public string MapType { get; set; }

        public string ClientId { get; set; }

        public string Source { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
