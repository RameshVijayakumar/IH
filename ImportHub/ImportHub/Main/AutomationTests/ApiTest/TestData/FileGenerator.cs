using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.TestData
{
    public static class FileGenerator
    {
        static readonly FileType filetype = new FileType();
        public static void SetFileType(FileType fileType)
        {
            filetype.Copy(fileType);
        }
        public static void ExportTestFile<T>(IEnumerable<T> data, string fileName) where T : IDataRecord, new()
        {
            if (String.IsNullOrEmpty(filetype.FileExtension) || String.IsNullOrEmpty(filetype.Delimiter))
            {
                throw new AutomationTestException("Failed to generate test file. File delimiter or file extension is not set.");
            }
            if (data == null || !data.Any())
            {
                throw new AutomationTestException("Failed to generate test file. No data to export.");
            }
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(Path.GetFullPath(baseDir), String.Format(@"{0}.{1}", fileName, filetype.FileExtension));
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var record in data)
                    {
                        writer.WriteLine(record.ToString(filetype.Delimiter));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new AutomationTestException(ex.StackTrace);
            }
        }
    }
}
