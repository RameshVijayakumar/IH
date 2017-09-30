using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IChunkData
    {
        ChunkDataResponse Create(ImportContext context, MappingDefinition map);

        int TotalRecordCount { get; set; }
    }
}