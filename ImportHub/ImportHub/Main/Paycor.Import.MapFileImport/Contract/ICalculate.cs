namespace Paycor.Import.MapFileImport.Contract
{
    public interface ICalculate
    {
        int GetFileRowNumber(int chunkSize, int chunkNumber, int chunkRecordNumber);

        double GetChunkStepPercent(StepNameEnum stepNameEnum, int totalChunks, double previousPercent = 0);
    }
}