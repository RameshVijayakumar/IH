using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net;
using Moq;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.MapFileImport.Implementation.Sender;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappedFileImportProcessOrchestratorTests
    {
        [TestClass]
        public class OrchestratorConstructorTests
        {
            private readonly Mock<ILog> _mockLogger;
            private readonly Mock<IChunkData> _mockChunker;
            private readonly Mock<IDataSourceBuilder> _mockBuilder;
            private readonly Mock<IPreparePayload> _mockPreparer;
            private readonly Mock<IPayloadSenderFactory> _mockSenderFactory;
            private readonly Mock<IReporter> _mockReporter;
            private readonly Mock<IStoreData<ImportCancelToken>> _mockStoreData;


            public OrchestratorConstructorTests()
            {
                _mockLogger = new Mock<ILog>();
                _mockChunker = new Mock<IChunkData>();
                _mockBuilder = new Mock<IDataSourceBuilder>();
                _mockPreparer = new Mock<IPreparePayload>();
                _mockSenderFactory = new Mock<IPayloadSenderFactory>();
                _mockReporter = new Mock<IReporter>();
                _mockStoreData = new Mock<IStoreData<ImportCancelToken>>();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Orchestrator_Enforce_Logger_Test()
            {
                var orchestrator = new MappedFileImportProcessOrchestrator(null,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);
                Assert.IsNull(orchestrator);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Orchestrator_Enforce_Chunker_Test()
            {
                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    null,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);
                Assert.IsNull(orchestrator);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Orchestrator_Enforce_Builder_Test()
            {
                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    null,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);
                Assert.IsNull(orchestrator);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Orchestrator_Enforce_Preparer_Test()
            {
                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    null,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);
                Assert.IsNull(orchestrator);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Orchestrator_Enforce_Sender_Test()
            {
                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    null,
                    _mockReporter.Object,
                    _mockStoreData.Object);
                Assert.IsNull(orchestrator);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Orchestrator_Enforce_Reporter_Test()
            {
                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    null,
                    _mockStoreData.Object);
                Assert.IsNull(orchestrator);
            }
        }

        [TestClass]
        public class OrchestratorProcessTests
        {
            private readonly Mock<ILog> _mockLogger;
            private readonly Mock<IChunkData> _mockChunker;
            private readonly Mock<IDataSourceBuilder> _mockBuilder;
            private readonly Mock<IPreparePayload> _mockPreparer;
            private readonly Mock<IPayloadSender> _mockSender;
            private readonly Mock<IPayloadSenderFactory> _mockSenderFactory;
            private readonly Mock<IReporter> _mockReporter;
            private readonly Mock<IStoreData<ImportCancelToken>> _mockStoreData;
            private readonly Mock<ILog> _log;
            private readonly Mock<IHttpInvoker> _httpInvoker;
            private readonly Mock<IGenerateFailedRecord> _generateFailedRecord;
            private readonly Mock<ICalculate> _calcuate;
            private readonly Mock<IApiExecutor> _apiExecutor;

            public OrchestratorProcessTests()
            {
                _mockLogger = new Mock<ILog>();
                _mockChunker = new Mock<IChunkData>();
                _mockBuilder = new Mock<IDataSourceBuilder>();
                _mockPreparer = new Mock<IPreparePayload>();
                _mockSender = new Mock<IPayloadSender>();
                _mockSenderFactory = new Mock<IPayloadSenderFactory>();
                _mockReporter = new Mock<IReporter>();
                _mockStoreData = new Mock<IStoreData<ImportCancelToken>>();
                _log = new Mock<ILog>();
                _httpInvoker = new Mock<IHttpInvoker>();
                _generateFailedRecord = new Mock<IGenerateFailedRecord>();
                _calcuate = new Mock<ICalculate>();
                _apiExecutor = new Mock<IApiExecutor>();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            public void Orchestrator_Process_1_Chunk()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Success,
                        DataSource = new Mock<IChunkDataSource>().Object
                    });

                _mockPreparer
                    .Setup(preparer => preparer.Prepare(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IChunkDataSource>()))
                    .Returns(() => new PreparePayloadResponse
                    {
                        Status = Status.Success,
                        PayloadDataItems = new List<PayloadData>
                        {
                            new PayloadData
                            {
                                EndPoint = "Bogus",
                                HtmlVerb = HtmlVerb.Post,
                                PayLoad = "Json Payload"
                            }
                        }
                    });

                _mockSenderFactory.Setup(s => s.GetSenderExtracter(It.IsAny<bool>()))
                    .Returns(
                        () =>
                            new PayloadSender(_log.Object, _apiExecutor.Object));

                _mockSender.Setup(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()))
                    .Returns(() =>
                    {
                        var response = new PayloadSenderResponse();
                        response.Status = Status.Success;
                        response.ApiLinks = new[] { "" };
                        return Task.FromResult(response);
                    });

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Once);

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Once);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Once);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()), Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(4));
            }

            [TestMethod]
            public void Orchestrator_Process_Batch_APIPreferred_Chunksize()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Success,
                        DataSource = new Mock<IChunkDataSource>().Object
                    });

                _mockPreparer
                    .Setup(preparer => preparer.Prepare(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IChunkDataSource>()))
                    .Returns(() => new PreparePayloadResponse
                    {
                        Status = Status.Success,
                        PayloadDataItems = new List<PayloadData>
                        {
                            new PayloadData
                            {
                                EndPoint = "Bogus",
                                HtmlVerb = HtmlVerb.Post,
                                PayLoad = "Json Payload"
                            }
                        }
                    });

                _mockSender.Setup(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()))
                    .Returns(() =>
                    {
                        var response = new PayloadSenderResponse();
                        response.Status = Status.Success;
                        response.ApiLinks = new[] { "" };
                        return Task.FromResult(response);
                    });

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = true,
                        IsBatchChunkingSupported = true,
                        PreferredBatchChunkSize = 25,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Once);

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Once);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Once);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()), Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(3));
            }

            [TestMethod]
            public void Orchestrator_Process_Batch_Default_Chunksize()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Success,
                        DataSource = new Mock<IChunkDataSource>().Object
                    });

                _mockPreparer
                    .Setup(preparer => preparer.Prepare(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IChunkDataSource>()))
                    .Returns(() => new PreparePayloadResponse
                    {
                        Status = Status.Success,
                        PayloadDataItems = new List<PayloadData>
                        {
                            new PayloadData
                            {
                                EndPoint = "Bogus",
                                HtmlVerb = HtmlVerb.Post,
                                PayLoad = "Json Payload"
                            }
                        }
                    });

                _mockSender.Setup(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()))
                    .Returns(() =>
                    {
                        var response = new PayloadSenderResponse();
                        response.Status = Status.Success;
                        response.ApiLinks = new[] { "" };
                        return Task.FromResult(response);
                    });

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = true,
                        IsBatchChunkingSupported = true,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Once);

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Once);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Once);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()), Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(3));
            }

            [TestMethod]
            public void Orchestrator_Process_Batch()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Success,
                        DataSource = new Mock<IChunkDataSource>().Object
                    });

                _mockPreparer
                    .Setup(preparer => preparer.Prepare(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IChunkDataSource>()))
                    .Returns(() => new PreparePayloadResponse
                    {
                        Status = Status.Success,
                        PayloadDataItems = new List<PayloadData>
                        {
                            new PayloadData
                            {
                                EndPoint = "Bogus",
                                HtmlVerb = HtmlVerb.Post,
                                PayLoad = "Json Payload"
                            }
                        }
                    });

                _mockSender.Setup(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()))
                    .Returns(() =>
                    {
                        var response = new PayloadSenderResponse();
                        response.Status = Status.Success;
                        response.ApiLinks = new[] { "" };
                        return Task.FromResult(response);
                    });

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = true,
                        IsBatchChunkingSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Once);

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Once);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Once);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()), Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(3));
            }


            [TestMethod]
            public void Orchestrator_Process_Fail_Chunker()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Failure
                    });

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);
                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Once);

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Never);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Never);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()),
                    Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(1));

                _mockReporter.Verify(reporter => reporter.ReportCompletionAsync(), Times.Exactly(1));

            }


            [TestMethod]
            public void Orchestrator_Process_Fail_Chunker_When_Null_Response()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => null);

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Exactly(1));

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Never);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Never);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()),
                    Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(1));

                _mockReporter.Verify(reporter => reporter.ReportCompletionAsync(), Times.Exactly(1));

            }


            [TestMethod]
            public void Orchestrator_Process_Fail_Builder()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Failure
                    });

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Once);

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Once);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Never);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()),
                    Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(2));

                _mockReporter.Verify(reporter => reporter.ReportCompletionAsync(), Times.Exactly(1));


            }

            [TestMethod]
            public void Orchestrator_Process_Fail_Builder_When_Null_Response()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                }
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => null);

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Exactly(1));

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Exactly(1));

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Never);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()),
                    Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(2));

                _mockReporter.Verify(reporter => reporter.ReportCompletionAsync(), Times.Exactly(1));
            }

            [TestMethod]
            public void Orchestrator_Process_Fail_Preparer()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Success,
                        DataSource = new Mock<IChunkDataSource>().Object
                    });

                _mockPreparer
                    .Setup(preparer => preparer.Prepare(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IChunkDataSource>()))
                    .Returns(() => new PreparePayloadResponse
                    {
                        Status = Status.Failure,
                    });

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Once);

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Once);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Once);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()),
                    Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(3));

                _mockReporter.Verify(reporter => reporter.ReportCompletionAsync(), Times.Exactly(1));


            }


            [TestMethod]
            public void Orchestrator_Process_Fail_Preparer_When_Null_Response()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Success,
                        DataSource = new Mock<IChunkDataSource>().Object
                    });

                _mockPreparer
                    .Setup(preparer => preparer.Prepare(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IChunkDataSource>()))
                    .Returns(() => null);


                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Exactly(1));

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Exactly(1));

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Exactly(1));

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()),
                    Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(3));

                _mockReporter.Verify(reporter => reporter.ReportCompletionAsync(), Times.Exactly(1));

            }


            [TestMethod]
            public void Orchestrator_Process_Fail_Sender()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Success,
                        DataSource = new Mock<IChunkDataSource>().Object
                    });

                _mockPreparer
                    .Setup(preparer => preparer.Prepare(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IChunkDataSource>()))
                    .Returns(() => new PreparePayloadResponse
                    {
                        Status = Status.Success,
                        PayloadDataItems = new List<PayloadData>
                        {
                            new PayloadData
                            {
                                EndPoint = "Bogus",
                                HtmlVerb = HtmlVerb.Post,
                                PayLoad = "Json Payload"
                            }
                        }
                    });

                _mockSender.Setup(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()))
                    .Returns(() =>
                    {
                        var response = new PayloadSenderResponse();
                        response.Status = Status.Failure;
                        return Task.FromResult(response);
                    });

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Once);

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Once);

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Once);

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()), Times.Never);

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(3));

                _mockReporter.Verify(reporter => reporter.ReportCompletionAsync(), Times.Exactly(1));


            }

            [TestMethod]
            public void Orchestrator_Process_Fail_Sender_When_Null_Response()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new ChunkDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<List<IDictionary<string, string>>>()
                        {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 1"},
                                    {"field 2", "value 2"},
                                    {"field 3", "value 3"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 4"},
                                    {"field 2", "value 5"},
                                    {"field 3", "value 6"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"field 1", "value 7"},
                                    {"field 2", "value 8"},
                                    {"field 3", "value 9"}
                                },
                            }
                        }
                    });

                _mockBuilder
                    .Setup(buildr => buildr.Build(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IEnumerable<IDictionary<string, string>>>()))
                    .Returns(() => new BuildDataSourceResponse
                    {
                        Status = Status.Success,
                        DataSource = new Mock<IChunkDataSource>().Object
                    });

                _mockPreparer
                    .Setup(preparer => preparer.Prepare(
                        It.IsAny<ImportContext>(),
                        It.IsAny<GeneratedMapping>(),
                        It.IsAny<IChunkDataSource>()))
                    .Returns(() => new PreparePayloadResponse
                    {
                        Status = Status.Success,
                        PayloadDataItems = new List<PayloadData>
                        {
                            new PayloadData
                            {
                                EndPoint = "Bogus",
                                HtmlVerb = HtmlVerb.Post,
                                PayLoad = "Json Payload"
                            }
                        }
                    });

                _mockSender.Setup(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()))
                    .Returns(() => null);

                var orchestrator = new MappedFileImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MappedImportFileUploadMessage
                {
                    ApiMapping = new GeneratedMapping
                    {
                        Mapping = new MappingDefinition(),
                        IsBatchSupported = false,
                        ObjectType = "Test",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        }
                    },
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<MappingDefinition>()), Times.Exactly(1));

                _mockBuilder.Verify(builder => builder.Build(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>()),
                    Times.Exactly(1));

                _mockPreparer.Verify(preparer => preparer.Prepare(
                    It.IsAny<ImportContext>(),
                    It.IsAny<GeneratedMapping>(),
                    It.IsAny<IChunkDataSource>()),
                    Times.Exactly(1));

                _mockSender.Verify(sendr => sendr.SendAsync(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IEnumerable<PayloadData>>()), Times.Exactly(0));

                _mockReporter.Verify(reporter => reporter.ReportAsync(
                    It.IsAny<StepNameEnum>(), It.IsAny<MapFileImportResponse>()), Times.Exactly(3));

                _mockReporter.Verify(reporter => reporter.ReportCompletionAsync(), Times.Exactly(1));
            }
        }
    }
}