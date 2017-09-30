using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.JsonFormat;
using Paycor.Import.Status;

namespace Paycor.Import.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class JsonFormatterTest
    {
        [TestMethod]
        public void Remove_Status_Details_Valid_Properties_To_Remove()
        {
            var formatter = new JsonFormatter();
            var formattedData =  formatter.RemoveProperties(File.ReadAllText("Json\\ImportStatus.json"), new List<string> {"StatusDetails"});
            var message = JsonConvert.DeserializeObject<MappedFileImportStatusMessage>(formattedData);

            Assert.IsTrue(message.StatusDetails.Count == 0);
        }

        [TestMethod]
        public void Remove_Status_Details_When_No_Status_Details_Exist()
        {
            var formatter = new JsonFormatter();
            formatter.RemoveProperties(File.ReadAllText("Json\\ImportStatusWithNoStatusDetails.json"), new List<string>() { "StatusDetails" });
        }

        [TestMethod]
        public void Remove_Status_Details_InValid_Properties()
        {
            var formatter = new JsonFormatter();
            formatter.RemoveProperties(File.ReadAllText("Json\\ImportStatusWithNoStatusDetails.json"), new List<string>()            {
                "Aasas" ,
                "asaas@@@#####"
            });
        }
    }
}
