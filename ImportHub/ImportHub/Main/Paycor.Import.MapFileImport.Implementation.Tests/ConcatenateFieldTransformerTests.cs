using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.MapFileImport.Implementation.Transformers;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ConcatenateFieldTransformerTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void Concatenate_FirstAndLastName_Success()
        {
            var mapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiConcatenationMapping.json"));
            var masterSession = Guid.NewGuid().ToString();

            var keyValueList = new List<KeyValuePair<string,string>>
            {
                new KeyValuePair<string, string>("person.firstName", "Value A"),
                new KeyValuePair<string, string>("person.lastName", "Value B"),
                new KeyValuePair<string, string>("tobacoUser", "true"),
                new KeyValuePair<string, string>("mobilePhone", "123")
            };

            var transformer = new ConcatenateFieldTransformer();
            var transformedData = transformer.TransformRecordFields(mapping.Mapping, masterSession, null, keyValueList).ToList();
            Assert.AreEqual(transformedData.First(t=>t.Key == "person.firstName").Value, "Value AValue B");
        }

        [TestMethod]
        public void TransformRecordFields_ValidationFails()
        {
            var mapping = new MappingDefinition();
            var masterSession = Guid.NewGuid().ToString();

            var record = new Dictionary<string, string>
                {
                    {"Column A", "Value A"},
                    {"Column B", "Value B"},
                    {"Column C", "BAD"},
                    {"Column D", "Not transformed"}
                };

            var transformer = new ConcatenateFieldTransformer();
            var payload = transformer.TransformRecordFields(mapping, masterSession, record).ToList();
            Assert.AreEqual(0, payload.Count);
        }
    }
}
