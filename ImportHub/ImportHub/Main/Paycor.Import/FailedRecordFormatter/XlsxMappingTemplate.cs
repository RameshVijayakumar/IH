using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using Paycor.Import.Extensions;
//TODO: Missing unit tests
namespace Paycor.Import.FailedRecordFormatter
{
    public class XlsxMappingTemplate : IXlsxRecordFormatter<string>
    {
        public byte[] GenerateXlsxData(IList<string> listOfHeaders)
        {
            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var excelPackage = new ExcelPackage(memoryStream))
                {
                    var worksheet = excelPackage.Workbook.Worksheets.Add("Template");

                    WriteHeaderForMappingTemplate(worksheet, listOfHeaders);
                    excelPackage.Save();
                }
                memoryStream.Flush();

                memoryStream.Position = 0;
                bytes = memoryStream.ToArray();
            }
            return bytes;
        }

        public byte[] GenerateXlsxData(IDictionary<string, IList<string>> failedRecordsDictionary)
        {
            throw new System.NotImplementedException();
        }

        private void WriteHeaderForMappingTemplate(ExcelWorksheet worksheet, IEnumerable<string> headers)
        {
            const int row = 1;
            var col = 1;
            foreach (var header in headers)
            {
                var headerCol = header;                
                if (headerCol.IsOrdinal())
                {
                    headerCol = "";
                }
                worksheet.Cells[row, col].Value = headerCol;
                col++;
            }
        }
    }
}
