using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.FailedRecordFormatter;

namespace Paycor.Import.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExcelToCsvConverterTests
    {
        [ExcludeFromCodeCoverage]
        [TestMethod]
        public void BasicFunctionality()
        {
            var bytes = File.ReadAllBytes("Excel\\Headers.xlsx");
            var converter = new ExcelToCsvConverter();
            var output = converter.Convert(bytes);
            var strVal = OutputAsString(output);
        }

        private string OutputAsString(byte[] inBytes)
        {
            var output = "";
            foreach (var inByte in inBytes)
            {
                output += (char) inByte;
            }

            return output;
        }
    }
}
