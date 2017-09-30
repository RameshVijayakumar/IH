using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Implementation.Transformers;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class GlobalAndClientAliasTransformerTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void TransformAliasRecordFieldsTest()
        {
            var log = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("Json\\GlobalAndClientAliasResponse.json"),
                        Encoding.UTF8, "application/json")
                    }

                });

            var globalAndClientAliasTransformer = new GlobalAndClientAliasTransformer(log.Object, httpInvoker.Object,"Uri");
            var record1 = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("employeeNumber", "(47&)"),
                new KeyValuePair<string, string>("WorkPhone", "(232-232-2345)")
            };
            var record2 = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("employeeType", "FT")
            };

            var records = new List<IEnumerable<KeyValuePair<string, string>>>
            {
                record1,record2
            };
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var transformedData = globalAndClientAliasTransformer.TransformAliasRecordFields(employeeMapping.Mapping, records, Guid.NewGuid().ToString());

            Assert.AreEqual("Full Time", transformedData.ElementAt(1).Where(t => t.Key == "employeeType").Select(t => t.Value).First());
        }

        [TestMethod]
        public void TransformAliasRecordFieldsTest_Handle_Exception_Properly()
        {
            var log = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
            )).Throws(new Exception());

            var globalAndClientAliasTransformer = new GlobalAndClientAliasTransformer(log.Object, httpInvoker.Object, "Uri");
            var record1 = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("employeeNumber", "(47&)"),
                new KeyValuePair<string, string>("WorkPhone", "(232-232-2345)")
            };
            var record2 = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("employeeType", "FT")
            };

            var records = new List<IEnumerable<KeyValuePair<string, string>>>
            {
                record1,record2
            };
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var transformedData = globalAndClientAliasTransformer.TransformAliasRecordFields(employeeMapping.Mapping, records, Guid.NewGuid().ToString());
        }
    }
}
