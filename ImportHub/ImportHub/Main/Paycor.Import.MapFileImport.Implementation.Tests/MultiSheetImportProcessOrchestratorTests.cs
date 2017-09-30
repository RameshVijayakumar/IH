using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.MapFileImport.Implementation.Sender;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MultiSheetImportProcessOrchestratorTests
    {
        [TestClass]
        public class OrchestratorConstructorTests
        {
            private readonly Mock<ILog> _mockLogger;
            private readonly Mock<IChunkMultiData> _mockChunker;
            private readonly Mock<IDataSourceBuilder> _mockBuilder;
            private readonly Mock<IPreparePayload> _mockPreparer;
            private readonly Mock<IPayloadSender> _mockSender;
            private readonly Mock<IPayloadSenderFactory> _mockSenderFactory;
            private readonly Mock<IReporter> _mockReporter;
            private readonly Mock<IStoreData<ImportCancelToken>> _mockStoreData;


            public OrchestratorConstructorTests()
            {
                _mockLogger = new Mock<ILog>();
                _mockChunker = new Mock<IChunkMultiData>();
                _mockBuilder = new Mock<IDataSourceBuilder>();
                _mockPreparer = new Mock<IPreparePayload>();
                _mockSender = new Mock<IPayloadSender>();
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
                var orchestrator = new MultiSheetImportProcessOrchestrator(null,
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
                var orchestrator = new MultiSheetImportProcessOrchestrator(
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
                var orchestrator = new MultiSheetImportProcessOrchestrator(
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
                var orchestrator = new MultiSheetImportProcessOrchestrator(
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
                var orchestrator = new MultiSheetImportProcessOrchestrator(
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
            private readonly Mock<IChunkMultiData> _mockChunker;
            private readonly Mock<IDataSourceBuilder> _mockBuilder;
            private readonly Mock<IPreparePayload> _mockPreparer;
            private readonly Mock<IPayloadSender> _mockSender;
            private readonly Mock<IPayloadSenderFactory>  _mockSenderFactory;
            private readonly Mock<IReporter> _mockReporter;
            private readonly Mock<IStoreData<ImportCancelToken>> _mockStoreData;

            public OrchestratorProcessTests()
            {
                _mockLogger = new Mock<ILog>();
                _mockChunker = new Mock<IChunkMultiData>();
                _mockBuilder = new Mock<IDataSourceBuilder>();
                _mockPreparer = new Mock<IPreparePayload>();
                _mockSender = new Mock<IPayloadSender>();
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
            public void Orchestrator_Process_1_Sheet()
            {
                var mappings = new []
                {
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 1",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "Employee",
                        Mapping = new MappingDefinition()
                    },
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 2",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "EmployeeDeduction",
                        Mapping = new MappingDefinition()
                    }
                };

                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new ChunkMultiDataResponse()
                    {
                        Status = Status.Success,
                        Chunks = new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>
                        {
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[0],
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
                                }),
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[1],
                                new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 1"},
                                        {"field 5", "value 2"},
                                        {"field 6", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 4"},
                                        {"field 5", "value 5"},
                                        {"field 6", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 7"},
                                        {"field 5", "value 8"},
                                        {"field 6", "value 9"}
                                    }
                                })
                        },
                        MultiSheetChunks = new List<SheetChunk>
                        {
                            new SheetChunk
                            {
                                ApiMapping = mappings[0],
                                ChunkTabData =  new List<IDictionary<string, string>>
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
                            },
                            new SheetChunk
                            {
                                ApiMapping = mappings[1],
                                ChunkTabData =  new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 1"},
                                        {"field 5", "value 2"},
                                        {"field 6", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 4"},
                                        {"field 5", "value 5"},
                                        {"field 6", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 7"},
                                        {"field 5", "value 8"},
                                        {"field 6", "value 9"}
                                    }
                                }
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

                var orchestrator = new MultiSheetImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MultiSheetImportStatusMessage()
                {
                    BaseMappings = mappings,
                    Container = "Bogus",
                    File = "Bogus",
                    MasterSessionId = "1",
                    TransactionId = "1",
                    UploadedFileName = "Double Bogey"
                };

                orchestrator.ProcessAsync(message);

                _mockChunker.Verify(chunk => chunk.Create(
                    It.IsAny<ImportContext>(),
                    It.IsAny<IList<ApiMapping>>()), Times.Once);

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
            }

            [TestMethod]
            public void Orchestrator_Process_Fail_Chunker()
            {
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new ChunkMultiDataResponse
                    {
                        Status = Status.Failure
                    });

                var orchestrator = new MultiSheetImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MultiSheetImportStatusMessage
                {
                    BaseMappings = new[]
                    {
                        new GeneratedMapping
                        {
                            Mapping = new MappingDefinition(),
                            IsBatchSupported = false,
                            MappingEndpoints = new MappingEndpoints
                            {
                                Delete = "DeleteRoute",
                                Post = "PostRoute"
                            }
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
                    It.IsAny<IList<ApiMapping>>()), Times.Once);

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
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => null);

                var orchestrator = new MultiSheetImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MultiSheetImportStatusMessage
                {
                    BaseMappings = new[]
                    {
                        new GeneratedMapping
                        {
                            Mapping = new MappingDefinition(),
                            IsBatchSupported = false,
                            MappingEndpoints = new MappingEndpoints
                            {
                                Delete = "DeleteRoute",
                                Post = "PostRoute"
                            }
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
                    It.IsAny<IList<ApiMapping>>()), Times.Exactly(1));

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
                var mappings = new[]
   {
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 1",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "Employee",
                        Mapping = new MappingDefinition()
                    },
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 2",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "EmployeeDeduction",
                        Mapping = new MappingDefinition()
                    }
                };

                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new ChunkMultiDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>
                        {
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[0],
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
                                }),
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[1],
                                new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 1"},
                                        {"field 5", "value 2"},
                                        {"field 6", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 4"},
                                        {"field 5", "value 5"},
                                        {"field 6", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 7"},
                                        {"field 5", "value 8"},
                                        {"field 6", "value 9"}
                                    }
                                })
                        },
                        MultiSheetChunks = new List<SheetChunk>
                        {
                            new SheetChunk
                            {
                                ApiMapping = mappings[0],
                                ChunkTabData =  new List<IDictionary<string, string>>
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
                            },
                            new SheetChunk
                            {
                                ApiMapping = mappings[1],
                                ChunkTabData = new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 1"},
                                        {"field 5", "value 2"},
                                        {"field 6", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 4"},
                                        {"field 5", "value 5"},
                                        {"field 6", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 7"},
                                        {"field 5", "value 8"},
                                        {"field 6", "value 9"}
                                    }
                                }
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

                var orchestrator = new MultiSheetImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MultiSheetImportStatusMessage()
                {
                    BaseMappings = new[]
                    {
                        new GeneratedMapping
                        {
                            Mapping = new MappingDefinition(),
                            IsBatchSupported = false,
                            MappingEndpoints = new MappingEndpoints
                            {
                                Delete = "DeleteRoute",
                                Post = "PostRoute"
                            }
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
                    It.IsAny<IList<ApiMapping>>()), Times.Once);

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
            public void Orchestrator_Process_Fail_Preparer()
            {
                var mappings = new[]
                {
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 1",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "Employee",
                        Mapping = new MappingDefinition()
                    },
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 2",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "EmployeeDeduction",
                        Mapping = new MappingDefinition()
                    }
                };
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new ChunkMultiDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>
                        {
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[0],
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
                                }),
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[1],
                                new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 1"},
                                        {"field 5", "value 2"},
                                        {"field 6", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 4"},
                                        {"field 5", "value 5"},
                                        {"field 6", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 7"},
                                        {"field 5", "value 8"},
                                        {"field 6", "value 9"}
                                    }
                                })
                        },
                        MultiSheetChunks = new List<SheetChunk>
                        {
                            new SheetChunk
                            {
                                ApiMapping = mappings[0],
                                ChunkTabData =  new List<IDictionary<string, string>>
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
                            },
                            new SheetChunk
                            {
                                ApiMapping = mappings[1],
                                ChunkTabData = new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 1"},
                                        {"field 5", "value 2"},
                                        {"field 6", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 4"},
                                        {"field 5", "value 5"},
                                        {"field 6", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 7"},
                                        {"field 5", "value 8"},
                                        {"field 6", "value 9"}
                                    }
                                }
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

                var orchestrator = new MultiSheetImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MultiSheetImportStatusMessage()
                {
                    BaseMappings = new [] {
                        new GeneratedMapping
                        {
                            Mapping = new MappingDefinition(),
                            IsBatchSupported = false,
                            MappingEndpoints = new MappingEndpoints
                            {
                                Delete = "DeleteRoute",
                                Post = "PostRoute"
                            }
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
                    It.IsAny<IList<ApiMapping>>()), Times.Once);

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
            public void Orchestrator_Process_Fail_sender()
            {
                var mappings = new[]
                {
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 1",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "Employee",
                        Mapping = new MappingDefinition()
                    },
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 2",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "EmployeeDeduction",
                        Mapping = new MappingDefinition()
                    }
                };
                _mockChunker
                    .Setup(chunkr => chunkr.Create(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new ChunkMultiDataResponse
                    {
                        Status = Status.Success,
                        Chunks = new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>
                        {
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[0],
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
                                }),
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[1],
                                new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 1"},
                                        {"field 5", "value 2"},
                                        {"field 6", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 4"},
                                        {"field 5", "value 5"},
                                        {"field 6", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 7"},
                                        {"field 5", "value 8"},
                                        {"field 6", "value 9"}
                                    }
                                })
                        },
                        MultiSheetChunks = new List<SheetChunk>
                        {
                            new SheetChunk
                            {
                                ApiMapping = mappings[0],
                                ChunkTabData =  new List<IDictionary<string, string>>
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
                            },
                            new SheetChunk
                            {
                                ApiMapping = mappings[1],
                                ChunkTabData = new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 1"},
                                        {"field 5", "value 2"},
                                        {"field 6", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 4"},
                                        {"field 5", "value 5"},
                                        {"field 6", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 4", "value 7"},
                                        {"field 5", "value 8"},
                                        {"field 6", "value 9"}
                                    }
                                }
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
                    });


                var orchestrator = new MultiSheetImportProcessOrchestrator(
                    _mockLogger.Object,
                    _mockChunker.Object,
                    _mockBuilder.Object,
                    _mockPreparer.Object,
                    _mockSenderFactory.Object,
                    _mockReporter.Object,
                    _mockStoreData.Object);

                var message = new MultiSheetImportStatusMessage()
                {
                    BaseMappings = new[] {
                        new GeneratedMapping
                        {
                            Mapping = new MappingDefinition(),
                            IsBatchSupported = false,
                            MappingEndpoints = new MappingEndpoints
                            {
                                Delete = "DeleteRoute",
                                Post = "PostRoute"
                            }
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
                    It.IsAny<IList<ApiMapping>>()), Times.Once);

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

        }
    }
}
