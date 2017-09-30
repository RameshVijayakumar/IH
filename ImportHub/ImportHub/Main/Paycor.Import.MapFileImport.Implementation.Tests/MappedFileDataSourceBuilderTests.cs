//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Runtime.Remoting.Messaging;
//using log4net;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Paycor.Import.MapFileImport.Contract;
//using Paycor.Import.Mapping;
//using Paycor.Integration.Mapping;
//
//namespace Paycor.Import.MapFileImport.Implementation.Tests
//{
//    [ExcludeFromCodeCoverage]
//    [TestClass]
//    public class MappedFileDataSourceBuilderTests
//    {
//
//        [TestClass]
//        public class ConstructorTests
//        {
//            private Mock<ILog> _mockLogger;
//            private readonly Mock<IEnumerable<ITransformRecordFields<MappingDefinition>>> _mockFieldTransformers;
//            private readonly Mock<ITransformAliasRecordFields<MappingDefinition>> _mockAliasTransformer;
//            private readonly Mock<IRecordSplitter<MappingDefinition>> _mockRecordSplitter;
//            private readonly Mock<ILookup> _mockLookup;
//
//            public ConstructorTests()
//            {
//                _mockLogger = new Mock<ILog>();
//                _mockFieldTransformers = new Mock<IEnumerable<ITransformRecordFields<MappingDefinition>>>();
//                _mockAliasTransformer = new Mock<ITransformAliasRecordFields<MappingDefinition>>();
//                _mockRecordSplitter = new Mock<IRecordSplitter<MappingDefinition>>();
//                _mockLookup = new Mock<ILookup>();
//            }
//
//            [TestCleanup]
//            public void TestCleanup()
//            {
//                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
//            }
//
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public void Builder_Enforces_Logger()
//            {
//                var builder = new MappedFileDataSourceBuilder(null,
//                    _mockFieldTransformers.Object,
//                    _mockAliasTransformer.Object,
//                    _mockRecordSplitter.Object,
//                    _mockLookup.Object); 
//                Assert.IsNull(builder);
//            }
//
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public void Builder_Enforces_FieldTransformers()
//            {
//                var builder = new MappedFileDataSourceBuilder(_mockLogger.Object,
//                    null,
//                    _mockAliasTransformer.Object,
//                    _mockRecordSplitter.Object,
//                    _mockLookup.Object);
//                Assert.IsNull(builder);
//            }
//
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public void Builder_Enforces_AliasTransfomer()
//            {
//                var builder = new MappedFileDataSourceBuilder(_mockLogger.Object,
//                    _mockFieldTransformers.Object,
//                    null,
//                    _mockRecordSplitter.Object,
//                    _mockLookup.Object);
//                Assert.IsNull(builder);
//            }
//
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public void Builder_Enforces_RecordSplitter()
//            {
//                var builder = new MappedFileDataSourceBuilder(_mockLogger.Object,
//                    _mockFieldTransformers.Object,
//                    _mockAliasTransformer.Object,
//                    null,
//                    _mockLookup.Object);
//                Assert.IsNull(builder);
//            }
//        }
//
//        [TestClass]
//        public class BuildTests
//        {
//            private Mock<ILog> _mockLogger;
//            private readonly Mock<IEnumerable<ITransformRecordFields<MappingDefinition>>> _mockFieldTransformers;
//            private readonly Mock<ITransformAliasRecordFields<MappingDefinition>> _mockAliasTransformer;
//            private readonly Mock<IRecordSplitter<MappingDefinition>> _mockRecordSplitter;
//            private readonly MappedFileDataSourceBuilder _builder;
//            private readonly Mock<ILookup> _mockLookup;
//
//            private GeneratedMapping _mapping = new GeneratedMapping
//            {
//                ChunkSize = 5,
//                DocUrl = "http://localhost/bogus",
//                HasHeader = true,
//                IsBatchSupported = false,
//                IsBatchChunkingSupported = false,
//                IsMappingValid = true,
//                Mapping = new MappingDefinition
//                {
//                    FieldDefinitions = new MappingFieldDefinition[]
//                    {
//                        new MappingFieldDefinition
//                        {
//                            Source = "Name",
//                            Destination = "Name",
//                            Required = false,
//                            SourceType = SourceTypeEnum.File,
//                            Type = "string",
//                            IsRequiredForPayload = false
//                        },
//                        new MappingFieldDefinition
//                        {
//                            Source = "Address",
//                            Destination = "Address",
//                            Required = false,
//                            SourceType = SourceTypeEnum.File,
//                            Type = "string",
//                            IsRequiredForPayload = false
//                        }
//                    }               
//                },
//                MappingEndpoints = new MappingEndpoints
//                {
//                    Post = "Post"
//                },
//                MappingName = "Bogus"
//            };
//            private List<IDictionary<string, string>> _chunk = new List<IDictionary<string, string>>();
//            private ImportContext _context = new ImportContext
//            {
//                HasHeader = true,
//                CallApiInBatch = false,
//                ChunkSize = 5,
//                ChunkNumber = 1,
//                MasterSessionId = "1",
//                TransactionId = "1",               
//            };
//
//            public BuildTests()
//            {
//                _mockLogger = new Mock<ILog>();
//                _mockFieldTransformers = new Mock<IEnumerable<ITransformRecordFields<MappingDefinition>>>();
//                _mockAliasTransformer = new Mock<ITransformAliasRecordFields<MappingDefinition>>();
//                _mockRecordSplitter = new Mock<IRecordSplitter<MappingDefinition>>();
//                _mockLookup = new Mock<ILookup>();
//
//                _builder = new MappedFileDataSourceBuilder(_mockLogger.Object,
//                    _mockFieldTransformers.Object,
//                    _mockAliasTransformer.Object,
//                    _mockRecordSplitter.Object,
//                    _mockLookup.Object);
//            }
//
//            [TestCleanup]
//            public void TestCleanup()
//            {
//                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
//            }
//
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public void Builder_Build_Enforces_ImportContext()
//            {
//                _builder.Build(null, _mapping, _chunk);
//            }
//
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public void Builder_Build_Enforces_ApiMapping()
//            {
//                _builder.Build(_context, null, _chunk);
//            }
//
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public void Builder_Build_Enforces_Chunk()
//            {
//                _builder.Build(_context, _mapping, null);
//            }
//
//            [TestMethod]
//            public void Builder_Build_Completes()
//            {
//                //TODO[Alan]: Figure this out with someone who knows mock better than me
//                //_chunk.Add(new Dictionary<string, string>
//                //{
//                //    {"Name", "Alan"},
//                //    {"Address", "PO Box 8008"}
//                //});
//                //_chunk.Add(new Dictionary<string, string>
//                //{
//                //    {"Name", "Ashif"},
//                //    {"Address", "1234 Hyde Ave"}
//                //});
//                var response = _builder.Build(_context, _mapping, _chunk);
//                Assert.IsTrue(response.Status == Status.Success);
//                Assert.IsNull(response.Error);
//                Assert.AreEqual(0, response.PayloadCount);
//                Assert.IsNotNull(response.DataSource);
//            }
//
//            [TestMethod]
//            public void Builder_Build_Fails()
//            {
//                _mockRecordSplitter.Setup(
//                    x =>
//                        x.TransformRecordsToDictionaryList(It.IsAny<MappingDefinition>(),
//                            It.IsAny<IEnumerable<IEnumerable<KeyValuePair<string, string>>>>())).Throws(new Exception("Bogus"));
//
//                var response = _builder.Build(_context, _mapping, _chunk);
//                Assert.IsTrue(response.Status == Status.Failure);
//                Assert.IsNotNull(response.Error);
//                Assert.AreEqual(0, response.PayloadCount);
//                Assert.IsNull(response.DataSource);
//            }
//        }
//    }
//}
