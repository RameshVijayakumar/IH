using System;

namespace Paycor.Import.ImportHubTest.ApiTest.TestData
{
    public class FileType
    {
        public string Delimiter { get; set; }
        public string FileExtension { get; set; }

        public FileType() { }

        public FileType(string delimiter, string fileExtension)
        {
            Delimiter = delimiter;
            FileExtension = fileExtension;
        }

        public void Copy(FileType fileType)
        {
            Delimiter = fileType.Delimiter;
            FileExtension = fileType.FileExtension;
        }

        public bool Equals(FileType fileType)
        {
            return Delimiter.Equals(fileType.Delimiter, StringComparison.CurrentCulture) ||
                   FileExtension.Equals(fileType.FileExtension, StringComparison.CurrentCulture);
        }
    }
}
