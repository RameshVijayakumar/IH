using System.IO;
using System.Text;
using OfficeOpenXml;
using Paycor.Import.Extensions;


namespace Paycor.Import.FailedRecordFormatter
{
    public class ExcelToCsvConverter : IExcelToCsv
    {
        public byte[] Convert(byte[] inBytes)
        {
            using (var stream = new MemoryStream(inBytes))
            {

                var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[1];
                var dimension = worksheet.Dimension;
                var columns = dimension.Columns;
                var rows = dimension.Rows;
                var sb = new StringBuilder();

                for (var row = 1; row <= rows; row++)
                {
                    for (var column = 1; column <= columns; column++)
                    {
                        var val = worksheet.GetValue<string>(row, column).CsvEscape();
                        sb.Append(val);
                        if (column < columns) sb.Append(",");
                    }
                    sb.AppendLine();
                }
                return sb.ToString().ToByteArray();
            }
        }
    }
}
