using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.MapFileImport.Implementation.LegacyShim;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MultiSheetDataChunkerTests
    {
        [TestClass]
        public class Chunker_Constructor_Tests
        {
            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Chunker_Enforce_Logger()
            {
                var chunkr = new MultiSheetDataChunker(
                    null,
                    new Mock<IMultiSheetImportImporter>().Object);
                Assert.IsNull(chunkr);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Chunker_Enforce_ImportImporter()
            {
                var chunkr = new MultiSheetDataChunker(
                    new Mock<ILog>().Object,
                    null);
                Assert.IsNull(chunkr);
            }
        }

        [TestClass]
        public class Chunker_Create_Tests
        {
            private readonly Mock<ILog> _mockLogger = new Mock<ILog>();
            private readonly Mock<IMultiSheetImportImporter> _importer = new Mock<IMultiSheetImportImporter>();
            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            public void Chunker_Create_Fails()
            {
                var chunkr = new MultiSheetDataChunker(_mockLogger.Object, _importer.Object);
                var context = new ImportContext
                {
                    FileName = "Bogus",
                    CallApiInBatch = false,
                    ChunkSize = 3,
                    Container = "Bogus",
                    MasterSessionId = "one",
                    UploadedFileName = "Bogus",
                    TransactionId = "one",
                    ColumnHeaders = new List<string>
                    {
                        "One",
                        "Two",
                        "Three"
                    },
                    ChunkNumber = 0,
                    Endpoints = new Dictionary<HtmlVerb, string>
                    {
                        {HtmlVerb.Post, "endpoint"}
                    },
                    HasHeader = true
                };

                var map = new List<ApiMapping>();

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Throws(new Exception());

                var actualResponse = chunkr.Create(context, map);

                Assert.AreEqual(Status.Failure, actualResponse.Status);
                Assert.IsNotNull(actualResponse.Error);
            }

            [TestMethod]
            public void Chunker_Create_Works()
            {
                var chunkr = new MultiSheetDataChunker(_mockLogger.Object, _importer.Object);
                var context = new ImportContext
                {
                    FileName = "Bogus",
                    CallApiInBatch = false,
                    ChunkSize = 3,
                    Container = "Bogus",
                    MasterSessionId = "one",
                    UploadedFileName = "Bogus",
                    TransactionId = "one",
                    ColumnHeaders = new List<string>
                        {
                            "One",
                            "Two",
                            "Three"
                        },
                    ChunkNumber = 0,
                    Endpoints = new Dictionary<HtmlVerb, string>
                        {
                            {HtmlVerb.Post, "endpoint"}
                        },
                    HasHeader = true
                };

                var map = new List<ApiMapping>();

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => null);

                var actualResponse = chunkr.Create(context, map);

                Assert.IsNull(actualResponse.Chunks);

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>());

                actualResponse = chunkr.Create(context, map);
                Assert.AreEqual(0, actualResponse.Chunks.Count);

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
                    },
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 3",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "EmployeeEarnings",
                        Mapping = new MappingDefinition()
                    }

                };

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>
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
                                })
                    });

                actualResponse = chunkr.Create(context, map);
                Assert.AreEqual(1, actualResponse.Chunks.Count);

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>
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
                    });

                actualResponse = chunkr.Create(context, map);
                Assert.AreEqual(2, actualResponse.Chunks.Count);

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<IList<ApiMapping>>()))
                    .Returns(() => new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>
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
                                        {"field 7", "value 1"},
                                        {"field 8", "value 2"},
                                        {"field 9", "value 3"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 7", "value 4"},
                                        {"field 8", "value 5"},
                                        {"field 9", "value 6"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"field 7", "value 7"},
                                        {"field 8", "value 8"},
                                        {"field 9", "value 9"}
                                    }
                                }),
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[2],
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
                    });

                actualResponse = chunkr.Create(context, map);
                Assert.AreEqual(3, actualResponse.Chunks.Count);
            }
        }
    }
}
