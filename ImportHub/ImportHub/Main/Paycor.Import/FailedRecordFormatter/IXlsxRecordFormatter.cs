using System.Collections.Generic;

namespace Paycor.Import.FailedRecordFormatter
{
    public interface IXlsxRecordFormatter<T>
    {
        byte[] GenerateXlsxData(IList<T> rows);
        byte[] GenerateXlsxData(IDictionary<string, IList<T>> failedRecordsDictionary);
    }
}