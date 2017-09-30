using System.Collections.Generic;
using System.IO;

namespace Paycor.Import.FileDataExtracter.LegacyShim
{
    public interface IProvideSheetData
    {
        int GetNumberOfSheets(Stream stream);

        string GetSheetName(int sheetNumber, Stream stream);

        IList<string> GetAllSheetNames(Stream stream);
    }
}