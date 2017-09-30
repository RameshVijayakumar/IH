using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.MapFileImport;
using Paycor.Import.MapFileImport.Implementation.LegacyShim;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExcelExtracterTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void Verify_SupportedFileTypes()
        {
            Assert.AreEqual(".xlsx", new XlsxDataExtracter().SupportedFileTypes());
        }

        [TestMethod]
        public void Check_NumberofSheets_For_Excel_With_TwoSheets()
        {
            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\PartialHeaders.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
                Assert.AreEqual(2,excelExtracter.GetNumberOfSheets(stream));
            }
        }

        [TestMethod]
        public void Check_NumberofSheets_For_Excel_With_OneSheet()
        {
            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\EmptyHeaders.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
                Assert.AreEqual(1, excelExtracter.GetNumberOfSheets(stream));
            }
        }

        [TestMethod]
        public void Check_NumberofSheets_For_Invalid_Excel()
        {
            using (var stream = new MemoryStream())
            {
                var excelExtracter = new XlsxDataExtracter();
                Assert.AreEqual(0, excelExtracter.GetNumberOfSheets(stream));
            }
        }

        [TestMethod]
        public void SheetName_Test_For_Excel_With_TwoSheets()
        {
            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\PartialHeaders.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
                Assert.AreEqual("FirstGamePartialHeaders", excelExtracter.GetSheetName(1,stream));
                Assert.AreEqual("asdasd", excelExtracter.GetSheetName(2, stream));

                Assert.AreEqual("", excelExtracter.GetSheetName(0, stream));
                Assert.AreEqual("", excelExtracter.GetSheetName(4, stream));
            }
        }

        [TestMethod]
        public void SheetName_Test_For_InvalidExcel()
        {
            using (var stream = new MemoryStream())
            {
                var excelExtracter = new XlsxDataExtracter();
                Assert.AreEqual("", excelExtracter.GetSheetName(1, stream));
                Assert.AreEqual("", excelExtracter.GetSheetName(2, stream));

                Assert.AreEqual("", excelExtracter.GetSheetName(0, stream));
                Assert.AreEqual("", excelExtracter.GetSheetName(4, stream));
            }
        }

        [TestMethod]
        public void Verify_GetAllSheetNames()
        {
            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\PartialHeaders.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
                var names = excelExtracter.GetAllSheetNames(stream);
                
                Assert.AreEqual("FirstGamePartialHeaders", names[0]);
                Assert.AreEqual("asdasd", names[1]);
                Assert.AreEqual(2, names.Count);
            }

        }

        [TestMethod]
        public void Extract_Partial_Header_File_From_Sheet2()
        {

            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\PartialHeaders.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
                var message = new ImportContext
                {
                    XlsxWorkSheetNumber = 2,
                    HasHeader = true
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

                var records = excelExtracter.ExtractData(message, mappingDefinition, stream);
                Assert.IsTrue(records.Count == 1);
                var record = records.FirstOrDefault();
                Assert.AreEqual(record["0"], "2");
                Assert.AreEqual(record["1"], "NFL17");
                Assert.AreEqual(record["publisher"], "AT");
                Assert.AreEqual(record["retailPrice"], "50");
                Assert.AreEqual(record["clientId"], "91970");

            }
        }

        [TestMethod]
        public void Extract_Partial_Header_File_With_Constant()
        {

            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\PartialHeaders.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
                var message = new ImportContext
                {
                    HasHeader = true,
                    XlsxWorkSheetNumber = 2
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


                var records = excelExtracter.ExtractData(message, mappingDefinition,stream);
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

            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\EmptyHeaders.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
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



                var records = excelExtracter.ExtractData(message, mappingDefinition,stream);
                Assert.IsTrue(records.Count == 1);
                var record = records.FirstOrDefault();
                Assert.AreEqual(record["0"], "2");
                Assert.AreEqual(record["1"], "NFL17");
                Assert.AreEqual(record["2"], "AT");
                Assert.AreEqual(record["3"], "50");
                Assert.AreEqual(record["8"], "91970");

            }
        }

        [TestMethod]
        public void Extract_Header_File()
        {

            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\Headers.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
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

                var records = excelExtracter.ExtractData(message, mappingDefinition, stream);
                Assert.IsTrue(records.Count == 21);
                var record = records.ElementAt(9);
                Assert.AreEqual(record["gameId"], "33");
                Assert.AreEqual(record["Title"], "NFL17");
                Assert.AreEqual(record["publisher"], "EA");
                Assert.AreEqual(record["retailPrice"], "250");
                Assert.AreEqual(record["clientid"], "9970");

            }
        }

        [TestMethod]
        public void Extract_Header_File_With_DuplicateHiddenColumns()
        {

            using (var stream = new MemoryStream(File.ReadAllBytes("Excel\\HeadersWithHiddenDuplicateColumns.xlsx")))
            {
                var excelExtracter = new XlsxDataExtracter();
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

                var records = excelExtracter.ExtractData(message, mappingDefinition, stream);
                Assert.IsTrue(records.Count == 21);
                var record = records.ElementAt(9);
                Assert.AreEqual(record["gameId"], "33");
                Assert.AreEqual(record["Title"], "NFL17");
                Assert.AreEqual(record["publisher"], "EA");
                Assert.AreEqual(record["retailPrice"], "250");
                Assert.AreEqual(record["clientid"], "9970");

            }
        }
    }
}
