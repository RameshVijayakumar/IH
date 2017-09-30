using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace EmployeeImportWorker.Test
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EmployeeImportTranslatorTest
    {
        class TestEmployeeImportTranslator : EmployeeImportTranslator
        {
            public RestApiPayload Payload { get; private set; }

            public TestEmployeeImportTranslator(ILog log, MappingDefinition mappingDefinition) : base(log, mappingDefinition)
            {
            }

            protected override RestApiPayload OnTranslate(FileTranslationData<IDictionary<string, string>> input)
            {
                Payload = base.OnTranslate(input);
                return Payload;
            }
        }

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestClass]
        public class EmployeeImportTranslator_Translate_Test
        {
            [TestInitialize]
            public void Initialize()
            {


            }

            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            public void Translate_Empty()
            {
                var log = new Mock<ILog>();
                var mappingDefinition = new MappingDefinition();
                var translationData = new FileTranslationData<IDictionary<string, string>>();
                var translator = new TestEmployeeImportTranslator(log.Object, mappingDefinition);
                var configSettings = new NameValueCollection();

                translator.Initialize(configSettings);
                translator.Translate(translationData);
                Assert.IsNull(translator.Payload);
            }

            public void Translate_With_Incorrect_Payload_Type()
            {
                var log = new Mock<ILog>();
                var mappingDefinition = new MappingDefinition();
                var translationData = new FileTranslationData<IDictionary<string, string>>();
                var translator = new TestEmployeeImportTranslator(log.Object, mappingDefinition);
                var configSettings = new NameValueCollection();

                translator.Initialize(configSettings);
                translator.Translate(translationData);
                Assert.IsNull(translator.Payload);
            }
        }
    }
}
