using System.Collections.Generic;
using System.Linq;

namespace Paycor.Import.Messaging
{
    public class RestApiPayload : IntegrationsPayload
    {
        public IEnumerable<IDictionary<string, string>> Records { get; set; }

        public override int RecordCount => Records?.Count() ?? 0;
        public string ApiEndpoint { get; set; }
    }
}
