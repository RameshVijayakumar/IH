using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.MapFileImport.Implementation.Transformers;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RecordSplitterTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void TransformRecordsToDictionaryList_For_NoDuplicate_Keys_Test()
        {
            var log = new Mock<ILog>();
            var recordSplitter = new RecordSplitter(log.Object);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));

            var records = new List<IEnumerable<KeyValuePair<string, string>>>
            {
                    new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("expectedStartDate", "08012016"),
                    new KeyValuePair<string, string>("reason", "fun"),
                    new KeyValuePair<string, string>("clientId", "65273"),
                    new KeyValuePair<string, string>("employeeNumber", "47"),
                    new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                }
            };
            var splitData = recordSplitter.TransformRecordsToDictionaryList(employeeMapping.Mapping, records);
            Assert.AreEqual(1,splitData.Count());
        }

        [TestMethod]
        public void TransformRecordsToDictionaryList_For_Duplicate_Keys_Test()
        {
            var log = new Mock<ILog>();
            var recordSplitter = new RecordSplitter(log.Object);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));

            var records = new List<IEnumerable<KeyValuePair<string, string>>>
            {
                    new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("jobTitle", "Dev"),
                    new KeyValuePair<string, string>("reason", "fun"),
                    new KeyValuePair<string, string>("reason", "sick"),
                    new KeyValuePair<string, string>("clientId", "65273"),
                    new KeyValuePair<string, string>("employeeNumber", "47"),
                    new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                }
            };
            var splitData = recordSplitter.TransformRecordsToDictionaryList(employeeMapping.Mapping, records);
            Assert.AreEqual(2, splitData.Count());
        }
    }
}
