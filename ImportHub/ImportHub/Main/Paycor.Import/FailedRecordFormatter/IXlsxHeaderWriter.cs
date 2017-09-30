using System.Collections.Generic;
using OfficeOpenXml;

namespace Paycor.Import.FailedRecordFormatter
{
    public interface IXlsxHeaderWriter<in T>
    {
        void WriteHeaderRecord(IEnumerable<T> rows);
        IEnumerable<string> GetColumnNames();
        IEnumerable<string> GetCustomColumnNames();
        IEnumerable<string> GetErrorColumnNames();
        IEnumerable<string> GetUnknownErrorColumnNames();
        int GetColumnIndex(string value);
        string AppendTextForFailedColumnName();
        void SetWorksheet(ExcelWorksheet excelWorksheet);

    }
}