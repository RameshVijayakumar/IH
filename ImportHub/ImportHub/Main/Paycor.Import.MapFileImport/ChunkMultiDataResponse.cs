using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport
{

    [ExcludeFromCodeCoverage]
    public class ChunkMultiDataResponse : MapFileImportResponse
    {
        // Chunk data a response is a list of mapping with its associated sheet data.
        public IList<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>> Chunks { get; set; }

        public IEnumerable<SheetChunk> MultiSheetChunks { get; set; }
    }

    public class SheetChunk
    {
        public IList<IDictionary<string, string>>  ChunkTabData { get; set; }
        public ApiMapping ApiMapping { get; set; }
    }
}
