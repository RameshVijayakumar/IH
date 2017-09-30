using System;
using Newtonsoft.Json;

namespace Paycor.Import.Messaging
{
    public abstract class IntegrationsPayload
    {
        public string Name { get; set; }
        public string MasterSessionId { get; set; }
        public string TransactionId { get; set; }

        public abstract int RecordCount { get; }

        public string FileType { get; set; }

        [JsonIgnore]
        public DateTime ProcessingStartTime { get; set; }
    }
}
