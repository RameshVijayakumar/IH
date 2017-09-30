using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    public class MappedFileDataChunkerTests
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
                var chunkr = new MappedFileDataChunker(
                    null,
                    new Mock<IMappedFileImportImporter>().Object);
                Assert.IsNull(chunkr);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Chunker_Enforce_ImportImporter()
            {
                var chunkr = new MappedFileDataChunker(
                    new Mock<ILog>().Object,
                    null);
                Assert.IsNull(chunkr);
            }
        }

        [TestClass]
        public class Chunker_Create_Tests
        {
            private readonly Mock<ILog> _mockLogger = new Mock<ILog>();
            private readonly Mock<IMappedFileImportImporter> _importer = new Mock<IMappedFileImportImporter>();

            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            public void Chunker_Create_Fails()
            {
                var chunkr = new MappedFileDataChunker(_mockLogger.Object, _importer.Object);
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

                var map = new MappingDefinition();

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Throws(new Exception());

                var actualResponse = chunkr.Create(context, map);

                Assert.AreEqual(Status.Failure, actualResponse.Status);
                Assert.IsNotNull(actualResponse.Error);
            }

            [TestMethod]
            public void Chunker_Create_Works()
            {
                var chunkr = new MappedFileDataChunker(_mockLogger.Object, _importer.Object);
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

                var map = new MappingDefinition();

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => null);

                var actualResponse = chunkr.Create(context, map);

                Assert.IsNull(actualResponse.Chunks);

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new IDictionary<string, string>[]
                    {

                    });

                actualResponse = chunkr.Create(context, map);
                Assert.AreEqual(0, actualResponse.Chunks.Count());

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new IDictionary<string, string>[]
                    {
                        new Dictionary<string, string>
                        {
                            {"One", "1"},
                            {"Two", "2"},
                            {"Three", "3"}
                        },
                        new Dictionary<string, string>
                        {
                            {"One", "4"},
                            {"Two", "5"},
                            {"Three", "6"}
                        },
                    });

                actualResponse = chunkr.Create(context, map);
                Assert.AreEqual(1, actualResponse.Chunks.Count());
                
                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new IDictionary<string, string>[]
                    {
                        new Dictionary<string, string>
                        {
                            {"One", "1"},
                            {"Two", "2"},
                            {"Three", "3"}
                        },
                        new Dictionary<string, string>
                        {
                            {"One", "4"},
                            {"Two", "5"},
                            {"Three", "6"}
                        },
                        new Dictionary<string, string>
                        {
                            {"One", "one"},
                            {"Two", "two"},
                            {"Three", "three"}
                        },
                    });

                actualResponse = chunkr.Create(context, map);
                Assert.AreEqual(1, actualResponse.Chunks.Count());

                _importer.Setup(imp => imp.Import(It.IsAny<ImportContext>(), It.IsAny<MappingDefinition>()))
                    .Returns(() => new IDictionary<string, string>[]
                    {
                        new Dictionary<string, string>
                        {
                            {"One", "1"},
                            {"Two", "2"},
                            {"Three", "3"}
                        },
                        new Dictionary<string, string>
                        {
                            {"One", "4"},
                            {"Two", "5"},
                            {"Three", "6"}
                        },
                        new Dictionary<string, string>
                        {
                            {"One", "one"},
                            {"Two", "two"},
                            {"Three", "three"}
                        },
                        new Dictionary<string, string>
                        {
                            {"One", "ONE"},
                            {"Two", "TWO"},
                            {"Three", "THREE"}
                        },
                    });

                actualResponse = chunkr.Create(context, map);
                Assert.AreEqual(2, actualResponse.Chunks.Count());
            }
        }
    }
}
