using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport
{
    [ExcludeFromCodeCoverage]
    public class PreparePayloadResponse : MapFileImportResponse
    {
        public IEnumerable<PayloadData> PayloadDataItems { get; set; }

    }
}
