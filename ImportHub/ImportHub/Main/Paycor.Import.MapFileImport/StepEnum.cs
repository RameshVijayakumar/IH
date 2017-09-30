namespace Paycor.Import.MapFileImport
{
    public enum StepPercentEnum
    {
        Chunker = 10,
        Builder = 40,
        Preparer = 10,
        Sender = 40
    }

    public enum StepNameEnum
    {
        Chunker,
        Builder,
        Preparer,
        Sender
    }
}
