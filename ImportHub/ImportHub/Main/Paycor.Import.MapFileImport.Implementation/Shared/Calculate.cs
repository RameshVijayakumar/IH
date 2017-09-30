using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation.Shared
{
    public class Calculate : ICalculate
    {
        public int GetFileRowNumber(int chunkSize, int chunkNumber, int chunkRecordNumber)
        {
            chunkNumber--;

            return chunkSize * chunkNumber + chunkRecordNumber;
        }

        public double GetChunkStepPercent(StepNameEnum stepNameEnum, int totalChunks, double previousPercent = 0)
        {
            double stepPercent;

            switch (stepNameEnum)
            {
                case StepNameEnum.Chunker:
                    stepPercent = (double)StepPercentEnum.Chunker;
                    return stepPercent;

                case StepNameEnum.Builder:
                    stepPercent = (double)StepPercentEnum.Builder;
                    break;
                case StepNameEnum.Preparer:
                    stepPercent = (double)StepPercentEnum.Preparer;
                    break;
                case StepNameEnum.Sender:
                    stepPercent = (double)StepPercentEnum.Sender;
                    break;
                default:
                    stepPercent = 0;
                    break;
            }

            var chunkPercentage = 100.0 / totalChunks * .01;
            var percent = stepPercent*chunkPercentage + previousPercent;

            return percent;
        }

    }
}
