using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IPreparePayload
    {
        PreparePayloadResponse Prepare(ImportContext context, ApiMapping mapping, IChunkDataSource dataSource);
    }
}
