using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.FileType;

namespace Paycor.Import.Tests.FileType
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FileTypeRecognizerTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void Data_OnThirdRow()
        {
            var rows = new List<List<string>>()
            {
                null,
                new List<string> {""},
                new List<string> {"ThirdRow"}
            };

            Assert.AreEqual(FileTypeRecognizer.IndexOfValidData(rows),2);
        }

        [TestMethod]
        public void Data_OnSecondRow()
        {
            var rows = new List<List<string>>()
            {
                new List<string> {""},
                new List<string> {"sec"},
                new List<string> {"ThirdRow"}
            };

            Assert.AreEqual(FileTypeRecognizer.IndexOfValidData(rows), 1);
        }

        [TestMethod]
        public void Rows_With_All_NullData()
        {
            var rows = new List<List<string>>()
            {
                null,
                null,
                null
            };

            Assert.AreEqual(FileTypeRecognizer.IndexOfValidData(rows), -1);
        }

        [TestMethod]
        public void NullData()
        {
            Assert.AreEqual(FileTypeRecognizer.IndexOfValidData(null), -1);
        }

        [TestMethod]
        public void AllEmptyRows()
        {
            var rows = new List<List<string>>()
            {
                new List<string> {""},
                new List<string> {""},
                new List<string> {""}
            };

            Assert.AreEqual(FileTypeRecognizer.IndexOfValidData(rows), -1);
        }

    }
}
