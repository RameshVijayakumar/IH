using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
//TODO: No unit tests
namespace Paycor.Import.FailedRecordFormatter
{
    public class FailedRowsStyle
    {
        public Color? BackGroundColor { get; set; }
        public Color FontColor { get; set; }
    }
    public class XlsxRecordWriter : IXlsxRecordWriter<FailedRecord>
    {
        private ExcelWorksheet _excelWorksheet;
        private IXlsxHeaderWriter<FailedRecord> _headerWriter;
        private int _currentRow = 2;

        public XlsxRecordWriter(IXlsxHeaderWriter<FailedRecord> headerWriter)
        {
            _headerWriter = headerWriter;
        }
        public void WriteDataRecords(IEnumerable<FailedRecord> failedRecords)
        {
            if (failedRecords == null) throw new ArgumentNullException(nameof(failedRecords));
            foreach (var failedRecord in failedRecords)
            {
                if (failedRecord != null)
                {
                    WriteCustomColumnValues(failedRecord);
                    WriteOriginalColumnValuesAndErrorIfExist(failedRecord);
                    WriteUnknownErrorColumnValues(failedRecord);
                }
                _currentRow++;
            }
        }

        public void AutoFitColumns()
        {
            _excelWorksheet?.Cells.AutoFitColumns(0);
        }

        public void SetWorksheet(ExcelWorksheet excelWorksheet)
        {
            _excelWorksheet = excelWorksheet;
            _currentRow = 2;
        }

        public void SetHeaderWriter(IXlsxHeaderWriter<FailedRecord> headerWriter)
        {
            _headerWriter = headerWriter;
        }

        private void WriteCustomColumnValues(FailedRecord failedRecord)
        {
            foreach (var customColumnName in _headerWriter.GetCustomColumnNames().Where(customColumnName => customColumnName != null))
            {
                LookupAndWriteCustomField(failedRecord, customColumnName);
            }
        }

        private void WriteUnknownErrorColumnValues(FailedRecord failedRecord)
        {
            foreach (var unknownErrorColumn in _headerWriter.GetUnknownErrorColumnNames().Where(unknownErrorColumn => unknownErrorColumn != null))
            {
                LookupAndWriteUnknownErrorField(failedRecord, unknownErrorColumn);
            }
        }

        private void WriteOriginalColumnValuesAndErrorIfExist(FailedRecord failedRecord)
        {
            foreach (var columnName in _headerWriter.GetColumnNames().Where(columnName => columnName != null))
            {
                LookupAndWriteOriginalField(failedRecord, columnName); 

                if (_headerWriter.GetErrorColumnNames().ToList().All(t => t != columnName))
                    continue;

                LookupAndWriteErrorField(failedRecord, columnName);
            }
        }

        private void LookupAndWriteErrorField(FailedRecord failedRecord, string columnName)
        {
            if (failedRecord.Errors == null) return;
            var keys = failedRecord.Errors?.Keys.Select(t => t).ToList();
            var recordKeyName = keys.Find(t => t == columnName);

            WriteFieldValue(failedRecord.Errors, columnName + _headerWriter.AppendTextForFailedColumnName(), 
                recordKeyName, 
                new FailedRowsStyle
                {
                    BackGroundColor = Color.White,
                    FontColor = Color.Red
                });
        }

        private void LookupAndWriteUnknownErrorField(FailedRecord failedRecord, string columnName)
        {
            if (failedRecord.Errors == null) return;
            var keys = failedRecord.Errors?.Keys.Select(t => t).ToList();
            var recordKeyName = keys.Find(t => t == columnName);

            WriteFieldValue(failedRecord.Errors, columnName + _headerWriter.AppendTextForFailedColumnName(), 
                recordKeyName, 
                new FailedRowsStyle
                {
                    BackGroundColor = Color.Yellow,
                    FontColor = Color.Red
                });
        }


        private void LookupAndWriteCustomField(FailedRecord failedRecord, string columnName)
        {
            if (failedRecord.CustomData == null) return;
            var keys = failedRecord.CustomData?.Keys.Select(t => t).ToList();
            var recordKeyName = keys.Find(t => t == columnName);

            var style = (columnName == ImportConstants.XlsxFailedRecordOtherErrors)
                ? new FailedRowsStyle
                {
                    BackGroundColor = Color.Yellow,
                    FontColor = Color.Red
                }
                : null;

            WriteFieldValue(failedRecord.CustomData, columnName, recordKeyName, style);
        }

        private void LookupAndWriteOriginalField(FailedRecord failedRecord, string columnName)
        {
            if (failedRecord.Record == null) return;
            var keys = failedRecord.Record?.Keys.Select(t => t).ToList();
            var recordKeyName = keys.Find(t => t == columnName);

            var style = new FailedRowsStyle {FontColor = Color.Black};
            string errorValue;
            if (failedRecord.Errors != null && failedRecord.Errors.TryGetValue(columnName,out errorValue))
            {
                style.BackGroundColor = Color.Yellow;
            }
            else
            {
                style.BackGroundColor = null;
            }

            WriteFieldValue(failedRecord.Record, columnName, recordKeyName, style);
        }

        private void WriteFieldValue(IDictionary<string, string> dictionary, string columnName, string recordKeyName, 
            FailedRowsStyle style)
        {
            string columnValue;
            var columnIndex = _headerWriter.GetColumnIndex(columnName);
            if (columnIndex == -1)
            {
                throw new Exception(
                    $"{nameof(XlsxRecordWriter)} : {nameof(WriteFieldValue)} Attempt to write data to non-existent column");
            }

            if (dictionary != null && (recordKeyName != null && dictionary.TryGetValue(recordKeyName, out columnValue)))
            {
                _excelWorksheet.Cells[_currentRow, columnIndex].Value = columnValue;
                if (style != null)
                {
                    SetStyle(style, columnIndex);
                }
            }
        }

        private void SetStyle(FailedRowsStyle style, int columnIndex)
        {
            _excelWorksheet.Cells[_currentRow, columnIndex].Style.Font.Color.SetColor(style.FontColor);

            if (style.BackGroundColor == null) return;
            _excelWorksheet.Cells[_currentRow, columnIndex].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            _excelWorksheet.Cells[_currentRow, columnIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _excelWorksheet.Cells[_currentRow, columnIndex].Style.Fill.BackgroundColor.SetColor((Color) style.BackGroundColor);
        }
    }
}
