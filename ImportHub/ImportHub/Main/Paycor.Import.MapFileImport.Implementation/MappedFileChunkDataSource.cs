using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation
{
    [ExcludeFromCodeCoverage]
    public class MappedFileChunkDataSource : IChunkDataSource
    {
        public IEnumerable<Dictionary<string, string>> Records { get; set; }
    }
}
