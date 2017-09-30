using System.Collections.Generic;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IChunkDataSource
    {
        IEnumerable<Dictionary<string, string>> Records { get; set; }
    }
}
