using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Contract
{
    [ExcludeFromCodeCoverage]
    public class PayloadData
    {
        public string EndPoint { get; set; }

        public HtmlVerb HtmlVerb { get; set; }

        public string PayLoad { get; set; }

        public ApiRecord ApiRecord { get; set; }

        public IEnumerable<ApiRecord> ApiRecords { get; set; }
    }
}
