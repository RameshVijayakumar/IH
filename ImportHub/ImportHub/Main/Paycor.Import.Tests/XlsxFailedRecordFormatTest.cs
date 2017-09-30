using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.FailedRecordFormatter;

namespace Paycor.Import.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class XlsxFailedRecordFormatterTest
    {

        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void Excel_With_Just_Header_And_Two_Sheets()
        {
            var header = new XlsxHeaderWriter();
            var dataWriter = new XlsxRecordWriter(header);

            var xlsxFailedRecordFormatter = new XlsxFailedRecordFormatter
                (
                header, dataWriter
                );
            

            var failedData = new List<FailedRecord>();

            var row1 = new FailedRecord()
            {
                Record = new Dictionary<string, string> { { "SSN", "" },
                                                          { "FirstName", "" },
                                                          { "LastName", "" }
                                                        }
                                                        ,
                Errors = new Dictionary<string, string> {
                                                          { "SSN", "" },
                                                          { "FirstName", "" },
                                                          { "LastName", "" },
                                                          {"ImportHubErrors",""}
                                                        }
            };

            var row2 = new FailedRecord()
            {
                Record = new Dictionary<string, string> { { "SSN", "" },
                                                          { "FirstName", "" },
                                                          { "LastName", "" }
                                                        }
                                                        ,
                Errors = new Dictionary<string, string> { { "SSN", "" },
                                                          { "FirstName", "" },
                                                          { "LastName", "" }
                                                        }
            };

            failedData.Add(row1);
            failedData.Add(row2);
            

            var data = new Dictionary<string, IList<FailedRecord>>
            {
                {"tab1", failedData},
                {"tab2",failedData }
            };

            var xlsxData = xlsxFailedRecordFormatter.GenerateXlsxData(data);

            File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"\ExcelWithHeader.xlsx", xlsxData);
        }

        [TestMethod]
        public void Excel_With_Header_And_Failure()
        {
            var header = new XlsxHeaderWriter();
            var dataWriter = new XlsxRecordWriter(header);

            var xlsxFailedRecordFormatter = new XlsxFailedRecordFormatter
                (
                header, dataWriter
                );

            var failedData = new List<FailedRecord>();

            var row1 = new FailedRecord()
            {
                Record = new Dictionary<string, string> { { "SSN", "123" },
                                                          { "FirstName", "" },
                                                          { "LastName", "VK" }
                                                        }
            };

            var row2 = new FailedRecord()
            {
                Record = new Dictionary<string, string> { { "SSN", "qwqw" },
                                                          { "FirstName", "" },
                                                          { "LastName", "34345" }
                                                        }
                                                        ,
                Errors = new Dictionary<string, string> { { "SSN", "should not contain letters" },
                                                          { "FirstName", "should not contain number" },
                                                          { "LastName", "should not contain number" } ,
                                                          {"ImportHubError","unhandled exception\tunauthorized access"}
                                                        }
            };

            var row3 = new FailedRecord()
            {
                Record = new Dictionary<string, string> { { "SSN", "" },
                                                          { "FirstName", "TT" },
                                                          { "LastName", "VK" }
                                                        }
            };

            failedData.Add(row1);
            failedData.Add(row2);
            failedData.Add(row3);
            failedData.Add(row3);
            failedData.Add(row3);
            failedData.Add(row2);

            var data = new Dictionary<string, IList<FailedRecord>>
            {
                {"tab1", failedData},
                {"tab2",failedData }
            };

            File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"\ExcelWithHeaderAndFailure.xlsx",
                xlsxFailedRecordFormatter.GenerateXlsxData(data));
        }

        [TestMethod]
        public void Excel_With_Just_Header_And_2_Record_No_Error_Data()
        {
            var header = new XlsxHeaderWriter();
            var dataWriter = new XlsxRecordWriter(header);

            var xlsxFailedRecordFormatter = new XlsxFailedRecordFormatter
                (
                header, dataWriter
                );

            var failedData = new List<FailedRecord>();

            var row1 = new FailedRecord()
            {
                Record = new Dictionary<string, string> { { "SSN", "123" },
                                                          { "FirstName", "RV" },
                                                          { "LastName", "VI" }
                                                        }

            };

            var row2 = new FailedRecord()
            {
                Record = new Dictionary<string, string> { { "SSN", "213" },
                                                          { "FirstName", "Al" },
                                                          { "LastName", "PI" }
                                                        }

            };

            failedData.Add(row1);
            failedData.Add(row2);
            failedData.Add(row2);
            failedData.Add(row2);
            failedData.Add(row2);
            failedData.Add(row2);
            failedData.Add(row2);
            failedData.Add(row2);
            failedData.Add(row2);
            failedData.Add(row2);
            failedData.Add(row2);




            File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"\ExcelWithHeaderAndTestData.xlsx",
                xlsxFailedRecordFormatter.GenerateXlsxData(failedData));
        }

        [TestMethod]
        public void Excel_With_Empty_Rows()
        {
            var header = new XlsxHeaderWriter();
            var dataWriter = new XlsxRecordWriter(header);

            var xlsxFailedRecordFormatter = new XlsxFailedRecordFormatter
                (
                header, dataWriter
                );

            var failedData = new List<FailedRecord>();

            var row1 = new FailedRecord();

            var row2 = new FailedRecord();

            failedData.Add(row1);
            failedData.Add(row2);

            File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"\ExcelWithHeaderAndEmptyRows.xlsx",
                (xlsxFailedRecordFormatter.GenerateXlsxData(failedData)));
        }

        [TestMethod]
        public void Excel_With_Rows_Empty_Collection()
        {
            var header = new XlsxHeaderWriter();
            var dataWriter = new XlsxRecordWriter(header);

            var xlsxFailedRecordFormatter = new XlsxFailedRecordFormatter
                (
                header, dataWriter
                );

            var failedData = new List<FailedRecord>();

            var row1 = new FailedRecord()
            {
                Record = new Dictionary<string, string>(),
                Errors = new Dictionary<string, string>()
            };

            var row2 = new FailedRecord()
            {
                Errors = new Dictionary<string, string>()
            };

            failedData.Add(row1);
            failedData.Add(row2);
            File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"\ExcelWithHeaderAndEmpty.xlsx",
                xlsxFailedRecordFormatter.GenerateXlsxData(failedData));
        }
    }
}
