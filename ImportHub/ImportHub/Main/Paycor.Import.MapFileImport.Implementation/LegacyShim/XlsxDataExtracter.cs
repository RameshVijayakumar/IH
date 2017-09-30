using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using Paycor.Import.Extensions;
using Paycor.Import.FileDataExtracter.LegacyShim;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    public class XlsxDataExtracter : IFileDataExtracter<ImportContext>, IProvideSheetData
    {

        private ExcelPackage _excelPackage;

        private ExcelWorksheet _currentWorkSheet;

        private int _processedRow;
        private int _totalRows;
        private int _currentWorkSheetMaxRows;
        private int _currentWorkSheetMaxColumns;

        public string SupportedFileTypes()
        {
            return ".xlsx";
        }

        public IList<IDictionary<string, string>> ExtractData(ImportContext context, MappingDefinition map, MemoryStream memoryStream)
        {
            Ensure.ThatArgumentIsNotNull(context, nameof(context));
            Ensure.ThatArgumentIsNotNull(map, nameof(map));
            Ensure.ThatArgumentIsNotNull(memoryStream, nameof(memoryStream));

            IList<IDictionary<string, string>> fieldDefinitionRecords = new List<IDictionary<string, string>>();
            try
            {
                var records = ReadAllRecords(memoryStream, context);

                _processedRow = 0;
                foreach (var record in records)
                {
                    _processedRow++;
                    AddRecordByFieldDefinition(map, record, fieldDefinitionRecords);
                }
            }
            catch (Exception ex)
            {
                throw new ChunkerException(_processedRow, _totalRows, $"A failure occurred while either reading the uploaded file at sheet: {_currentWorkSheet}", _currentWorkSheet.ToString(), ex);
            }
            return fieldDefinitionRecords;
        }

        private static void AddRecordByFieldDefinition(MappingDefinition map, IReadOnlyDictionary<string, string> record, ICollection<IDictionary<string, string>> records)
        {
            if (record == null)
                return;

            var formattedRecord = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in map.FieldDefinitions.Where(field => field.SourceType != SourceTypeEnum.Const))
            {
                if (field.Source == null && field.Destination!=null)
                {
                    formattedRecord[field.Destination] = null;
                    continue;
                }

                var sourceList = field.Source.RemoveWhiteSpaces().SplitbyPipe();

                foreach (var source in sourceList)
                {
                    string columnValue;
                    if (!record.TryGetValue(source, out columnValue))
                    {
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(field.Source))
                    {
                        formattedRecord[source] = columnValue.Trim();
                    }
                }
            }
            records.Add(formattedRecord);
        }

        private IEnumerable<Dictionary<string, string>> ReadAllRecords(Stream memoryStream, ImportContext message)
        {
            Initialize(memoryStream, message.HasHeader,message.XlsxWorkSheetNumber);
            var dataTable = GetDataTableWithAllColumns(message.HasHeader);
            AddRowsToDataTable(dataTable, message.HasHeader);
            return GetRecordsFromDataTable(dataTable);
        }

        private void Initialize(Stream memoryStream, bool hasHeader, int workSheetNumber)
        {
            _excelPackage = new ExcelPackage(memoryStream);
            _currentWorkSheet = workSheetNumber == 0 ? _excelPackage.Workbook.Worksheets[1] :
                                                       _excelPackage.Workbook.Worksheets[workSheetNumber];
            DeleteEmptyRows(_currentWorkSheet);
            _processedRow = 1;
            _currentWorkSheetMaxRows = _currentWorkSheet.Dimension.End.Row;
            _currentWorkSheetMaxColumns = _currentWorkSheet.Dimension.End.Column;

            _totalRows += _currentWorkSheetMaxRows - (hasHeader ? 1 : 0);
        }

        private static void DeleteEmptyRows(ExcelWorksheet workSheet)
        {
            if (workSheet.Dimension == null)
            {
                return;
            }

            for (var rowNum = 1; rowNum <= workSheet.Dimension.End.Row; rowNum++)
            {
                var row = workSheet.Cells[$"{rowNum}:{rowNum}"];
                var isRowEmpty = row.All(c => string.IsNullOrWhiteSpace(c.Text));
                if (!isRowEmpty)
                    continue;
                workSheet.DeleteRow(rowNum);
                rowNum--;
            }
        }

        private static IEnumerable<Dictionary<string, string>> GetRecordsFromDataTable(DataTable dataTable)
        {
            var records = new List<Dictionary<string, string>>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (DataColumn dc in dr.Table.Columns)
                {
                    if (!record.ContainsKey(dc.ToString()))
                    {
                        record.Add(dc.ColumnName, dr[dc].ToString());
                    }
                }
                records.Add(record);
            }
            return records;
        }

        private DataTable GetDataTableWithAllColumns(bool hasHeader)
        {
            var dataTable = new DataTable(_currentWorkSheet.Name);
            for (var columnNumber = 0; columnNumber < _currentWorkSheetMaxColumns; columnNumber++)
            {
                string columnName;
                if (hasHeader)
                {
                    columnName = _currentWorkSheet.Cells[1, columnNumber + 1].Text;
                    if (string.IsNullOrWhiteSpace(columnName))
                    {
                        columnName = columnNumber.ToString();
                    }
                }
                else
                {
                    columnName = columnNumber.ToString();
                }

                if (!_currentWorkSheet.Column(columnNumber + 1).Hidden)
                dataTable.Columns.Add(columnName.RemoveWhiteSpaces());
            }
            return dataTable;
        }

        private void AddRowsToDataTable(DataTable dataTable, bool hasHeader)
        {
            var startingRowNumber = 1;
            if (hasHeader)
            {
                startingRowNumber = 2;
            }
            _processedRow = 0;
            for (var rowNumber = startingRowNumber; rowNumber <= _currentWorkSheetMaxRows; rowNumber++)
            {
                _processedRow++;
                var dataRow = dataTable.NewRow();
                var index = 0;
                for (var columnNumber = 0; columnNumber < _currentWorkSheetMaxColumns; columnNumber++)
                {
                    if (_currentWorkSheet.Column(columnNumber + 1).Hidden) continue;
                    dataRow[dataTable.Columns[index++]] = _currentWorkSheet.Cells[rowNumber, columnNumber + 1].Text;
                }
                dataTable.Rows.Add(dataRow);
            }
        }

        public int GetNumberOfSheets(Stream stream)
        {
            var excelPackage = new ExcelPackage(stream);

            return excelPackage.Workbook?.Worksheets?.Count ?? -1;
        }

        public string GetSheetName(int sheetNumber, Stream stream)
        {
            if (sheetNumber <= 0) return string.Empty;
            if (sheetNumber > GetNumberOfSheets(stream)) return string.Empty;
            var excelPackage = new ExcelPackage(stream);
            return excelPackage.Workbook?.Worksheets?[sheetNumber]?.Name ?? string.Empty;
        }


        public IList<string> GetAllSheetNames(Stream stream)
        {
            var totalSheets = GetNumberOfSheets(stream);

            var sheetNames = new List<string>();

            for (var i = 1; i <= totalSheets; i++)
            {
                sheetNames.Add(GetSheetName(i, stream));
            }

            return sheetNames;
        }

    }
}
