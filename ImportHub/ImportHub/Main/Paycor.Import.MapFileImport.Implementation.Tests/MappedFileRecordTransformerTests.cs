using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    public class MappedFileRecordTransformerTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void TransformRecordTest()
        {
            var log = new Mock<ILog>();

            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                    }

                });

            var globalLookupDefinition = new GlobalLookupDefinition
            {
                LookupTypeName = "employeeType",
                LookupTypeValue = new Dictionary<string, string>
                {
                    {"FT","Full Time" }
                }
            };
            var globalLookupReader = new Mock<IGlobalLookupTypeReader>();
            globalLookupReader.Setup(t => t.LookupDefinition("employeeType")).Returns(globalLookupDefinition);

            var globalLookupTransformer = new GlobalLookupTypeTransformer(globalLookupReader.Object);
            var fieldtransformers = new List<ITransformFields<MappingDefinition>> { globalLookupTransformer };

            var recordTransformer = new MappedFileRecordTransformer(log.Object, fieldtransformers);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));

            var record = new Dictionary<string,string>
            {
                { "expectedStartDate","08012016"},
                { "reason","fun"},
                { "clientId","65273"},
                { "employeeNumber","47"},
                { "WorkPhone","232-232-2345"},
                {"employeeType","FT"}
            };

            var transformedData = recordTransformer.TransformRecord(employeeMapping.Mapping, record, new Guid().ToString());
            Assert.AreEqual("Full Time", transformedData["employeeType"]);
        }

        [TestMethod]
        public void TransformRecordTest_Handles_Exception_Properly()
        {
            var log = new Mock<ILog>();

            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                    }

                });

            var globalLookupReader = new Mock<IGlobalLookupTypeReader>();
            globalLookupReader.Setup(t => t.LookupDefinition("employeeType")).Throws(new Exception());

            var globalLookupTransformer = new GlobalLookupTypeTransformer(globalLookupReader.Object);
            var fieldtransformers = new List<ITransformFields<MappingDefinition>> { globalLookupTransformer };

            var recordTransformer = new MappedFileRecordTransformer(log.Object, fieldtransformers);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));

            var record = new Dictionary<string, string>
            {
                { "expectedStartDate","08012016"},
                { "reason","fun"},
                { "clientId","65273"},
                { "employeeNumber","47"},
                { "WorkPhone","232-232-2345"},
                {"employeeType","FT"}
            };

            var transformedData = recordTransformer.TransformRecord(employeeMapping.Mapping, record, new Guid().ToString());
            Assert.AreEqual(0, transformedData.Count);
        }
    }
}
