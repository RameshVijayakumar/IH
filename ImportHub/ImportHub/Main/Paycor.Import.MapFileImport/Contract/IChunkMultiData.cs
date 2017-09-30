using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IChunkMultiData
    {
        ChunkMultiDataResponse Create(ImportContext context, IList<ApiMapping> mappings);

        int TotalRecordCount { get; set; }
    }
}