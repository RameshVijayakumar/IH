using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Paycor.Import.Extensions;

//TODO: Missing unit tests
namespace Paycor.Import.FileType
{
    public class FileTypeRecognizer
    {
        public static FileTypeEnum DetermineFormat(string firstLine)
        {
            Ensure.ThatArgumentIsNotNull(firstLine, nameof(firstLine));

            var fields = firstLine.Split(ImportConstants.Tab);
            if (fields.GetUpperBound(0) > 1)
            {
                return FileTypeEnum.EmployeeImport;
            }

            fields = firstLine.Split(ImportConstants.Comma);
            return fields.GetUpperBound(0) > 0 ? FileTypeEnum.MappedFileImport : FileTypeEnum.Unrecognized;
        }

        public static int IndexOfValidData(IEnumerable<IEnumerable<string>> payloadList)
        {
            var index = 0;
            if (payloadList == null)
            {
                return -1;
            }

            foreach (var payLoad in payloadList)
            {
                if (payLoad != null && !string.IsNullOrWhiteSpace(payLoad.FirstOrDefault()))
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        public static int GetMaxNumberOfColumns(IEnumerable<string> fileRowsArr, char delimiter)
        {
            var maxNumberOfColumns = 0;
            if (fileRowsArr == null)
            {
                return maxNumberOfColumns;
            }

            foreach (var line in fileRowsArr)
            {
                if (line == null) continue;
                var count = line.GetNumberofCommaSeperatedData();
                if (count > maxNumberOfColumns)
                    maxNumberOfColumns = count;
            }
            return maxNumberOfColumns;
        }
    }

    public enum FileTypeEnum 
    {
        Unrecognized = 0,
        EmployeeImport = 1,
        MappedFileImport = 2
    }
}