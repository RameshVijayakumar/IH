using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Paycor.Import.MapFileImport
{
    [ExcludeFromCodeCoverage]
    public class PayloadSenderResponse : MapFileImportResponse
    {
        public IEnumerable<string> ApiLinks { get; set; }
    }
}
