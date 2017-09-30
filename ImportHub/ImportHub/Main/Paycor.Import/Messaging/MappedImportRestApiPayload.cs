using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Paycor.Import.Messaging
{
    [ExcludeFromCodeCoverage]
    public sealed class MappedImportRestApiPayload : RestApiPayload
    {
        public bool CallApiInBatch { get; set; }
        [DataMember]
        public Dictionary<HtmlVerb, string> Endpoints { get; set; }
        public List<ApiRecord> ApiRecords { get; set; }
    }
}