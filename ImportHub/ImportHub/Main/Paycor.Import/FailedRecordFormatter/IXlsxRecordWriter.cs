using System.Collections.Generic;
using OfficeOpenXml;

namespace Paycor.Import.FailedRecordFormatter
{
    public interface IXlsxRecordWriter<T>
    {
        void WriteDataRecords(IEnumerable<T> rows);
        void AutoFitColumns();
        void SetWorksheet(ExcelWorksheet excelWorksheet);
        void SetHeaderWriter(IXlsxHeaderWriter<T> headerWriter);
    }
}