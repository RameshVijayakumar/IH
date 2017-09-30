using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
//TODO: No unit tests
namespace Paycor.Import.FailedRecordFormatter
{
    public class XlsxHeaderWriter : IXlsxHeaderWriter<FailedRecord>
    {
        private List<string> _columnNames;
        private List<string> _customColumnNames;
        private List<string> _errorColumnNames;
        private List<string> _unknownErrorColumnNames;
        private readonly Dictionary<string, int> _columnHeaderIndexAndNames;

        private ExcelWorksheet _excelWorksheet;
        private int _currentCell = 1;

        private const string AppendToFailedColumnName = "Errors";
        private const string ErrorColumnNameSeparator = " ";

        public XlsxHeaderWriter()
        {
            _columnHeaderIndexAndNames = new Dictionary<string, int>();
        }

        public void WriteHeaderRecord(IEnumerable<FailedRecord> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            _columnNames = new List<string>();
            _customColumnNames = new List<string>();
            _errorColumnNames = new List<string>();
            _unknownErrorColumnNames = new List<string>();

            var columnNames = new List<string>();
            var failureColumnNames = new List<string>();
            var customColumnNames = new List<string>();

            foreach (var row in rows)
            {
                if (row.Record != null)
                    columnNames = columnNames.Concat(row.Record.Keys.ToList()).ToList();
                if (row.Errors != null)
                    failureColumnNames = failureColumnNames.Concat(row.Errors.Keys.ToList()).ToList();
                if (row.CustomData != null)
                    customColumnNames = customColumnNames.Concat(row.CustomData.Keys.ToList()).ToList();
            }

            SetHeader(columnNames.Select(t => t).Distinct().ToList(),
                   failureColumnNames.Select(t => t).Distinct().ToList(),
                   customColumnNames.Select(t => t).Distinct().ToList()
                   );
        }


        private void SetHeader(List<string> columnNames, List<string> failureColumnNames, IEnumerable<string> customColumnNames)
        {
            var errorKeys = columnNames.Intersect(failureColumnNames).ToList();
            var unknownErrorKeys = failureColumnNames.Except(errorKeys).ToList();
            var noErrorKeys = columnNames.Except(errorKeys).ToList();

            WriteCustomColumns(customColumnNames);
            WriteHeaderColumnsAndErrorColumns(errorKeys, noErrorKeys);
            WriteUnKnownErrorColumns(unknownErrorKeys);
        }

        private void WriteUnKnownErrorColumns(IEnumerable<string> unknownErrorColumnNames)
        {
            if (unknownErrorColumnNames == null) return;
            foreach (var unknownErrorColumn in unknownErrorColumnNames.Where(unknownErrorColumn => unknownErrorColumn != null))
            {
                WriteHeader(unknownErrorColumn + AppendTextForFailedColumnName());
                _unknownErrorColumnNames.Add(unknownErrorColumn);
            }
        }

        private void WriteCustomColumns(IEnumerable<string> customColumnNames)
        {
            if (customColumnNames == null) return;
            foreach (var customColumn in customColumnNames.Where(customColumn => customColumn != null))
            {
                WriteHeader(customColumn);
                _customColumnNames.Add(customColumn);
            }
        }

        private void WriteHeaderColumnsAndErrorColumns(IEnumerable<string> errorKeys, IEnumerable<string> noErrorKeys)
        {
            if (errorKeys != null) WriteColumnsWithErrors(errorKeys);
            if (noErrorKeys != null) WriteColumnsWithNoError(noErrorKeys);
        }

        private void WriteColumnsWithErrors(IEnumerable<string> errorKeys)
        {
            if (errorKeys == null) return;
            foreach (var errorKey in errorKeys.Where(errorKey => errorKey != null))
            {
                WriteHeader(errorKey);
                _columnNames.Add(errorKey);

                WriteHeader(errorKey + AppendTextForFailedColumnName());
                _errorColumnNames.Add(errorKey);
            }
        }

        private void WriteColumnsWithNoError(IEnumerable<string> noErrorKeys)
        {
            if (noErrorKeys == null) return;
            foreach (var noErrorKey in noErrorKeys.Where(noErrorKey => noErrorKey != null))
            {
                WriteHeader(noErrorKey);
                _columnNames.Add(noErrorKey);
            }
        }

        private void WriteHeader(string value)
        {
            _columnHeaderIndexAndNames[value] =  _currentCell;
            _excelWorksheet.Cells[1, _currentCell++].Value = value;
        }

        public IEnumerable<string> GetColumnNames()
        {
            return _columnNames;
        }

        public IEnumerable<string> GetCustomColumnNames()
        {
            return _customColumnNames;
        }

        public IEnumerable<string> GetErrorColumnNames()
        {
            return _errorColumnNames;
        }

        public IEnumerable<string> GetUnknownErrorColumnNames()
        {
            return _unknownErrorColumnNames;
        }

        public int GetColumnIndex(string value)
        {
            if (!_columnHeaderIndexAndNames.ContainsKey(value)) return -1;
            int index;
            if (_columnHeaderIndexAndNames.TryGetValue(value, out index))
                return index;
            return -1;
        }

        public string AppendTextForFailedColumnName()
        {
            return ErrorColumnNameSeparator + AppendToFailedColumnName;
        }

        public void SetWorksheet(ExcelWorksheet excelWorksheet)
        {
            _excelWorksheet = excelWorksheet;
            _currentCell = 1;
        }
    }
}
