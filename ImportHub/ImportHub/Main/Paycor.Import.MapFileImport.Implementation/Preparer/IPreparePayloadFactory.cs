namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public enum PreparePayloadTypeEnum
    {
        Batch,
        NonBatch
    }
    public interface IPreparePayloadFactory
    {
        void LoadHandlers();
        IPayloadExtracter GetPayloadExtracter(PreparePayloadTypeEnum preparePayloadTypeEnum);
    }
}
