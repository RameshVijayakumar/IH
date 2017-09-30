using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Paycor.Import.MapFileImport
{
    [ExcludeFromCodeCoverage]
    public class ChunkDataResponse : MapFileImportResponse
    {
        public IEnumerable<IEnumerable<IDictionary<string, string>>> Chunks { get; set; }
    }
}
