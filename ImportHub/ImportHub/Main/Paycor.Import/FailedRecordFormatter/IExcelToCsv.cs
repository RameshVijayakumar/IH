namespace Paycor.Import.FailedRecordFormatter
{
    public interface IExcelToCsv
    {
        byte[] Convert(byte[] inBytes);
    }
}
