using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace Paycor.Import.FailedRecordFormatter
{
    public class XlsxFailedRecordFormatter : IXlsxRecordFormatter<FailedRecord>
    {
        private readonly IXlsxHeaderWriter<FailedRecord> _headerWriter;
        private readonly IXlsxRecordWriter<FailedRecord> _recordWriter;
        public XlsxFailedRecordFormatter(IXlsxHeaderWriter<FailedRecord> headerWriter, IXlsxRecordWriter<FailedRecord> recordWriter)
        {
            _recordWriter = recordWriter;
            _headerWriter = headerWriter;
        }
        public byte[] GenerateXlsxData(IList<FailedRecord> rows)
        {
            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var excelPackage = new ExcelPackage(memoryStream))
                {
                    if (rows != null)
                    {
                        AddWorkSheet(rows.ToList(), excelPackage,"Failed Records");
                        excelPackage.Save();
                    }
                }

                memoryStream.Flush();
                memoryStream.Position = 0;
                bytes = memoryStream.ToArray();
            }
            return bytes;
        }

        private void AddWorkSheet(IList<FailedRecord> rows, ExcelPackage excelPackage, string sheetName)
        {
            var worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);

            FormatFailedRecordDictionaryKeysToLower(rows);
            WriteHeader(rows, worksheet);
            WriteRecords(rows, worksheet);
        }

        public byte[] GenerateXlsxData(IDictionary<string, IList<FailedRecord>> failedRecordsDictionary)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var excelPackage = new ExcelPackage(memoryStream))
                {
                    foreach (var failedRecords in failedRecordsDictionary)
                    {
                        if (failedRecords.Value == null || string.IsNullOrWhiteSpace(failedRecords.Key)) continue;
                            AddWorkSheet(failedRecords.Value, excelPackage, failedRecords.Key);
                    }
                    excelPackage.Save();
                }
                memoryStream.Flush();
                memoryStream.Position = 0;
                bytes = memoryStream.ToArray();
            }
            return bytes;
        }
        private void WriteRecords(IEnumerable<FailedRecord> rows, ExcelWorksheet worksheet)
        {
            _recordWriter.SetHeaderWriter(_headerWriter);
            _recordWriter.SetWorksheet(worksheet);
            _recordWriter.WriteDataRecords(rows);
            _recordWriter.AutoFitColumns();
        }

        private void WriteHeader(IEnumerable<FailedRecord> rows, ExcelWorksheet worksheet)
        {
            _headerWriter.SetWorksheet(worksheet);
            _headerWriter.WriteHeaderRecord(rows);
        }

        private static void FormatFailedRecordDictionaryKeysToLower(IEnumerable<FailedRecord> rows)
        {
            foreach (var failedRecord in rows)
            {
                if (failedRecord.Errors != null)
                    failedRecord.Errors = failedRecord.Errors.ToDictionary(t => t.Key, t => t.Value, StringComparer.OrdinalIgnoreCase);

                if (failedRecord.CustomData != null)
                    failedRecord.CustomData = failedRecord.CustomData.ToDictionary(t => t.Key, t => t.Value, StringComparer.OrdinalIgnoreCase);

                if (failedRecord.Record != null)
                    failedRecord.Record = failedRecord.Record.ToDictionary(t => t.Key, t => t.Value, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
