using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;

namespace Paycor.Import.Tests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class KeyValuePairExtensionsTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void AddOrOverwriteKeyValuePairTest()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("employeeId", "ffg-try-hdgf")
            };

            record = record.AddOrOverwriteKeyValuePair(new KeyValuePair<string, string>("employeeId", "Abc-Def-dfklju")).ToList();
            Assert.AreEqual(record[3].Value, "Abc-Def-dfklju");
        }

        [TestMethod]
        public void AddOrOverwriteKeyValuePairCaseSensitiveIgnore_Test()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("EMPLOYEEID", "ffg-try-hdgf")
            };

            record = record.AddOrOverwriteKeyValuePair(new KeyValuePair<string, string>("employeeId", "Abc-Def-dfklju")).ToList();
            Assert.AreEqual(record[3].Value, "Abc-Def-dfklju");
        }

        [TestMethod]
        public void AddOrOverwriteKeyValuePairNULL_Test()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("employeeid", null)
            };

            record = record.AddOrOverwriteKeyValuePair(new KeyValuePair<string, string>("employeeId", "Abc-Def-dfklju")).ToList();
            Assert.AreEqual(record[3].Value, "Abc-Def-dfklju");
        }

        [TestMethod]
        public void AddOrOverwriteKeyValuePairResultNULL_Test()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("employeeid", "ffg-try-hdgf")
            };

            record = record.AddOrOverwriteKeyValuePair(new KeyValuePair<string, string>("employeeId", null)).ToList();
            Assert.AreEqual(record[3].Value, null);
        }

        [TestMethod]
        public void Test_IsRecordContainDuplicateKey()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("workphone", "232-232-2344")
            };

            var result = record.IsRecordContainDuplicateKey();
            Assert.AreEqual(true, result);

            var records = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("employeeId", "232-232-2344")
            };

            var result1 = records.IsRecordContainDuplicateKey();
            Assert.AreEqual(false, result1);

            var moreRecords = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("WORKPHONE", "232-232-2344"),
                new KeyValuePair<string, string>("EMAIL", "232-232-2344"),
                new KeyValuePair<string, string>("email", "232-232-2344")
            };

            var result2 = moreRecords.IsRecordContainDuplicateKey();
            Assert.AreEqual(true, result2);
        }

        [TestMethod]
        public void Test_GetNonDuplicateRowItems()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("workphone", "232-232-2344")
            };

            var result = record.GetNonDuplicateRowItems().ToList();
            Assert.AreEqual("clientId", result[0].Key);
            Assert.AreEqual("employeeNumber", result[1].Key);

        }

        [TestMethod]
        public void Test_GetDuplicateRowItems()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("workphone", "232-232-2344")
            };

            var result = record.GetDuplicateRowItems().ToList();
            Assert.AreEqual("WorkPhone", result[0].Key);
            
        }


        [TestMethod]
        public void Test_RecordContainsKey()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "477"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("workphone", "232-232-2344")
            };

            var result = record.RecordContainsKey("clientId");
            Assert.AreEqual(true, result);

            var result1 = record.RecordContainsKey("employeeId");
            Assert.AreEqual(false, result1);

        }

        [TestMethod]
        public void ConcatenateRecordFields()
        {
            var record = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("FirstName", "A"),
                new KeyValuePair<string, string>("LastName", "B"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("workphone", "232-232-2344")
            };

            var concatenatedData = record.ConcatenateRecordFields(new List<string> {"FirstName", "LastName"});
            Assert.AreEqual("AB", concatenatedData);

            var record1 = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("FirstName", ""),
                new KeyValuePair<string, string>("LastName", "B"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("workphone", "232-232-2344")
            };

            var concatenatedData1 = record1.ConcatenateRecordFields(new List<string> { "FirstName", "LastName","MiddleName" });
            Assert.AreEqual("B", concatenatedData1);
        }
    }
}
