using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.Azure;
using Newtonsoft.Json;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SwaggerDocRegistrationProcessorTests
    {
        [TestClass]
        public class ConstructorTests
        {
            private readonly Mock<ILog> _mockLog;
            private readonly Mock<IDocumentDbRepository<GeneratedMapping>> _mockRepo;
            private readonly Mock<IGlobalLookupTypeReader> _mockTypeReader;
            private readonly Mock<ISwaggerParser> _mockSwaggerParser;
            private readonly Mock<IApiMappingStatusService> _mockMappingService;
            private readonly Mock<IMappingCertification> _mockMappingCertification;

            public ConstructorTests()
            {
                _mockLog = new Mock<ILog>();
                _mockRepo = new Mock<IDocumentDbRepository<GeneratedMapping>>();
                _mockTypeReader = new Mock<IGlobalLookupTypeReader>();
                _mockSwaggerParser = new Mock<ISwaggerParser>();
                _mockMappingService = new Mock<IApiMappingStatusService>();
                _mockMappingCertification = new Mock<IMappingCertification>();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Constructor_Ensures_Repository()
            {
                var processor = new SwaggerDocRegistrationProcessor(
                    null,
                    _mockLog.Object,
                    _mockTypeReader.Object,
                    _mockSwaggerParser.Object,
                    _mockMappingService.Object,
                    _mockMappingCertification.Object
                    );
                Assert.IsNull(processor);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Constructor_Ensures_Log()
            {
                var processor = new SwaggerDocRegistrationProcessor(
                    _mockRepo.Object,
                    null,
                    _mockTypeReader.Object,
                    _mockSwaggerParser.Object,
                    _mockMappingService.Object,
                    _mockMappingCertification.Object
                    );
                Assert.IsNull(processor);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Constructor_Ensures_LookupTypeReader()
            {
                var processor = new SwaggerDocRegistrationProcessor(
                    _mockRepo.Object,
                    _mockLog.Object,
                    null,
                    _mockSwaggerParser.Object,
                    _mockMappingService.Object,
                    _mockMappingCertification.Object
                    );
                Assert.IsNull(processor);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Constructor_Ensures_SwaggerParser()
            {
                var processor = new SwaggerDocRegistrationProcessor(
                    _mockRepo.Object,
                    _mockLog.Object,
                    _mockTypeReader.Object,
                    null,
                    _mockMappingService.Object,
                    _mockMappingCertification.Object
                    );
                Assert.IsNull(processor);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Constructor_Ensures_MappingStatusService()
            {
                var processor = new SwaggerDocRegistrationProcessor(
                    _mockRepo.Object,
                    _mockLog.Object,
                    _mockTypeReader.Object,
                    _mockSwaggerParser.Object,
                    null,
                    _mockMappingCertification.Object
                    );
                Assert.IsNull(processor);
            }

        }

        [TestClass]
        public class ProcessTests
        {
            private readonly Mock<ILog> _mockLog;
            private readonly Mock<IDocumentDbRepository<GeneratedMapping>> _mockRepo;
            private readonly Mock<IGlobalLookupTypeReader> _mockTypeReader;
            private readonly Mock<ISwaggerParser> _mockSwaggerParser;
            private readonly Mock<IApiMappingStatusService> _mockMappingService;
            private readonly Mock<IMappingCertification> _mockMappingCertification;

            public ProcessTests()
            {
                _mockLog = new Mock<ILog>();
                _mockRepo = new Mock<IDocumentDbRepository<GeneratedMapping>>();
                _mockTypeReader = new Mock<IGlobalLookupTypeReader>();
                _mockSwaggerParser = new Mock<ISwaggerParser>();
                _mockMappingService = new Mock<IApiMappingStatusService>();
                _mockMappingCertification = new Mock<IMappingCertification>();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            //[TestMethod]
            public void ProcessRunsAndRegisters()
            {
                var mappingFactory = new RegistrationMappingGeneratorFactory();
                var swaggerParser = new SwaggerParser(mappingFactory);

                var processor = new SwaggerDocRegistrationProcessor(
                    _mockRepo.Object,
                    _mockLog.Object,
                    _mockTypeReader.Object,
                    swaggerParser,
                    _mockMappingService.Object,_mockMappingCertification.Object
                   );

                Assert.IsNotNull(processor);
                var docUrl = "http://localhost/referenceapi/swagger/docs/v3";

                _mockMappingService.Setup(e => e.GetApiStatusInfo(It.IsAny<string>()))
                    .Returns(() => new ApiMappingStatusInfo
                    {
                        DocUrl = docUrl,
                        Id = "1234",
                        LastRegistered = DateTime.Today
                    });
                _mockMappingService.Setup(e => e.IsRecentlyProcessed(It.IsAny<ApiMappingStatusInfo>()))
                    .Returns(() => false);
                var mappingsForDocUrl = JsonConvert.DeserializeObject<IEnumerable<GeneratedMapping>>(File.ReadAllText("json\\MappingsForV3ReferenceApi.json"));
                _mockRepo.Setup(e => e.GetItems(It.IsAny<Expression<Func<GeneratedMapping, bool>>>())).Returns(
                    () => mappingsForDocUrl);
                processor.Process(docUrl);
                _mockMappingService.Verify(e => e.UpdateApiStatusInfo(It.IsAny<ApiMappingStatusInfo>()),Times.Once);
            }

            [TestMethod]
            public void ProcessorRunsAndAborts()
            {
                var processor = new SwaggerDocRegistrationProcessor(
                    _mockRepo.Object,
                    _mockLog.Object,
                    _mockTypeReader.Object,
                    _mockSwaggerParser.Object,
                    _mockMappingService.Object,_mockMappingCertification.Object);

                Assert.IsNotNull(processor);
                var docUrl = "http://localhost/referenceapi/swagger/docs/v3";

                _mockMappingService.Setup(e => e.GetApiStatusInfo(It.IsAny<string>()))
                    .Returns(() => new ApiMappingStatusInfo
                    {
                        DocUrl = docUrl,
                        Id = "1234",
                        LastRegistered = DateTime.Now
                    });
                _mockMappingService.Setup(e => e.IsRecentlyProcessed(It.IsAny<ApiMappingStatusInfo>()))
                    .Returns(() => true);

                processor.Process(docUrl);
                _mockMappingService.Verify(e => e.UpdateApiStatusInfo(It.IsAny<ApiMappingStatusInfo>()), Times.Never);
            }

            //[TestMethod]
            public void ProcessorRunsAndErrorsInDelete()
            {
                var processor = new SwaggerDocRegistrationProcessor(
                    _mockRepo.Object,
                    _mockLog.Object,
                    _mockTypeReader.Object,
                    _mockSwaggerParser.Object,
                    _mockMappingService.Object,_mockMappingCertification.Object);
                Assert.IsNotNull(processor);
                var docUrl = "http://localhost/referenceapi/swagger/docs/v3";

                _mockMappingService.Setup(e => e.GetApiStatusInfo(It.IsAny<string>()))
                    .Returns(() => new ApiMappingStatusInfo
                    {
                        DocUrl = docUrl,
                        Id = "1234",
                        LastRegistered = DateTime.Today
                    });
                _mockMappingService.Setup(e => e.IsRecentlyProcessed(It.IsAny<ApiMappingStatusInfo>()))
                    .Returns(() => false);

                var mappingsForDocUrl = JsonConvert.DeserializeObject<IEnumerable<GeneratedMapping>>(File.ReadAllText("json\\MappingsForV3ReferenceApi.json"));
                _mockRepo.Setup(e => e.GetItems(It.IsAny<Expression<Func<GeneratedMapping, bool>>>())).Returns(
                    () => mappingsForDocUrl);
                _mockRepo.Setup(e => e.DeleteItemAsync(It.IsAny<string>())).Throws(new Exception("Test Exception"));

                processor.Process(docUrl);
                _mockMappingService.Verify(e => e.UpdateApiStatusInfo(It.IsAny<ApiMappingStatusInfo>()), Times.Once);
                _mockLog.Verify(e => e.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);
            }

            [TestMethod]
            public void ProcessorRunsAndErrors()
            {
                var processor = new SwaggerDocRegistrationProcessor(
                    _mockRepo.Object,
                    _mockLog.Object,
                    _mockTypeReader.Object,
                    _mockSwaggerParser.Object,
                    _mockMappingService.Object, _mockMappingCertification.Object);
                Assert.IsNotNull(processor);
                var docUrl = "http://localhost/referenceapi/swagger/docs/v3";

                _mockMappingService.Setup(e => e.GetApiStatusInfo(It.IsAny<string>()))
                    .Returns(() => new ApiMappingStatusInfo
                    {
                        DocUrl = docUrl,
                        Id = "1234",
                        LastRegistered = DateTime.Today
                    });
                _mockMappingService.Setup(e => e.IsRecentlyProcessed(It.IsAny<ApiMappingStatusInfo>()))
                    .Throws(new Exception("Test Exception"));

                var mappingsForDocUrl = JsonConvert.DeserializeObject<IEnumerable<GeneratedMapping>>(File.ReadAllText("json\\MappingsForV3ReferenceApi.json"));
                _mockRepo.Setup(e => e.GetItems(It.IsAny<Expression<Func<GeneratedMapping, bool>>>())).Returns(
                    () => mappingsForDocUrl);

                processor.Process(docUrl);
                _mockMappingService.Verify(e => e.UpdateApiStatusInfo(It.IsAny<ApiMappingStatusInfo>()), Times.Never);
                _mockLog.Verify(e => e.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);
            }
        }
    }
}
