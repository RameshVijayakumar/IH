using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.MapFileImport;
using Paycor.Import.MapFileImport.Implementation.LegacyShim;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CsvDataExtracterTests
    {
        [TestClass]
        public class SupportedFileTypesTests
        {
            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            public void CsvExtracter_VerifySupportedFileTypes()
            {
                var mockLogger = new Mock<ILog>();
                var extracter = new CsvDataExtracter(mockLogger.Object);
                Assert.AreEqual(".csv", extracter.SupportedFileTypes().Split(ImportConstants.Comma).ToList().FirstOrDefault());
            }
        }

        [TestClass]
        public class ConstructorTests
        {
            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Constructor_Enforces_Logger()
            {
                var extracter = new CsvDataExtracter(null);
                Assert.IsNull(extracter);
            }
        }
        
        [TestClass]
        public class ExtractDataTests
        {
            private readonly Mock<ILog> _mockLogger;

            public ExtractDataTests()
            {
                _mockLogger = new Mock<ILog>();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            public void Extract_Header_File()
            {

                using (var stream = new MemoryStream(File.ReadAllBytes("Csv\\Headers.csv")))
                {
                    var extracter = new CsvDataExtracter(_mockLogger.Object);
                    var message = new ImportContext
                    {
                        HasHeader = true
                    };

                    var mappingDefinition = new MappingDefinition
                    {
                        FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Source = "gameId",
                            Destination = "gameId",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "Title",
                            Destination = "Title",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "publisher",
                            Destination = "publisher",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "retailPrice",
                            Destination = "retailPrice",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "clientId",
                            Destination = "clientId",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        }
                    }
                    };

                    var records = extracter.ExtractData(message, mappingDefinition, stream);
                    Assert.IsTrue(records.Count == 20);
                    var record = records.ElementAt(9);
                    Assert.AreEqual(record["gameId"], "3");
                    Assert.AreEqual(record["Title"], "NFL17");
                    Assert.AreEqual(record["publisher"], "AT");
                    Assert.AreEqual(record["retailPrice"], "150");
                    Assert.AreEqual(record["clientid"], "91970");

                }
            }

            [TestMethod]
            public void Extract_Partial_Header_File_With_Constant()
            {

                using (var stream = new MemoryStream(File.ReadAllBytes("Csv\\PartialHeaders.csv")))
                {
                    var extracter = new CsvDataExtracter(_mockLogger.Object);
                    var message = new ImportContext
                    {
                        HasHeader = true,
                    };

                    var mappingDefinition = new MappingDefinition()
                    {
                        FieldDefinitions = new List<MappingFieldDefinition>()
                    {
                        new MappingFieldDefinition
                        {
                            Source = "0",
                            Destination = "gameId",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "1",
                            Destination = "Title",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "publisher",
                            Destination = "publisher",
                            SourceType = SourceTypeEnum.Const,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "retailPrice",
                            Destination = "retailPrice",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "clientId",
                            Destination = "clientId",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        }
                    }
                    };


                    var records = extracter.ExtractData(message, mappingDefinition, stream);
                    Assert.IsTrue(records.Count == 1);
                    var record = records.FirstOrDefault();
                    Assert.AreEqual(record["0"], "2");
                    Assert.AreEqual(record["1"], "NFL17");
                    Assert.AreEqual(record["retailPrice"], "50");
                    Assert.AreEqual(record["clientId"], "91970");

                }
            }

            [TestMethod]
            public void Extract_EmptyHeader_File()
            {

                using (var stream = new MemoryStream(File.ReadAllBytes("Csv\\EmptyHeaders.csv")))
                {
                    var extracter = new CsvDataExtracter(_mockLogger.Object);
                    var message = new ImportContext
                    {
                        HasHeader = false
                    };

                    var mappingDefinition = new MappingDefinition
                    {
                        FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Source = "0",
                            Destination = "gameId",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "1",
                            Destination = "Title",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "2",
                            Destination = "publisher",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "3",
                            Destination = "retailPrice",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "8",
                            Destination = "clientId",
                            SourceType = SourceTypeEnum.File,
                            Type = "string"
                        }
                    }
                    };



                    var records = extracter.ExtractData(message, mappingDefinition, stream);
                    Assert.IsTrue(records.Count == 1);
                    var record = records.FirstOrDefault();
                    Assert.AreEqual(record["0"], "2");
                    Assert.AreEqual(record["1"], "NFL17");
                    Assert.AreEqual(record["2"], "AT");
                    Assert.AreEqual(record["3"], "50");
                    Assert.AreEqual(record["8"], "91970");

                }
            }
        }
    }
}
