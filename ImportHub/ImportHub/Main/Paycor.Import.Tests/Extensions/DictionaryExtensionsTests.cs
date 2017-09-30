using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.Tests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DictionaryExtensionsTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void ConcatenateData_Success()
        {
            var failedRowsDictionary = new Dictionary<string,IList<FailedRecord>>
            {
                {"FirstKey", new List<FailedRecord> {new FailedRecord
                {
                    CustomData = new ConcurrentDictionary<string, string>(),
                    Record = new ConcurrentDictionary<string, string>(),
                    Errors = new ConcurrentDictionary<string, string>()
                } } }
            };

            failedRowsDictionary.ConcatenateData("FirstKey",new List<FailedRecord>()
            {
                new FailedRecord
                {
                    CustomData = new ConcurrentDictionary<string, string>()
                }
            });

            Assert.AreEqual(1, failedRowsDictionary.Count);
            Assert.AreEqual(2, failedRowsDictionary["FirstKey"].Count);

            var failedRowsDictionary1 = new Dictionary<string, IList<FailedRecord>>();

            failedRowsDictionary1.ConcatenateData("SecondKey", new List<FailedRecord>
            {
                new FailedRecord
                {
                    CustomData = new ConcurrentDictionary<string, string>()
                },
                new FailedRecord()
            });

            Assert.AreEqual(1,failedRowsDictionary1.Count);
            Assert.AreEqual(2, failedRowsDictionary1["SecondKey"].Count);

        }


        [TestMethod]
        public void ConcatenateData_NullData_To_SameKey()
        {
            var failedRowsDictionary4 = new Dictionary<string, IList<FailedRecord>>
            {
                {"FourthKey", new List<FailedRecord> {new FailedRecord
                {
                    CustomData = new ConcurrentDictionary<string, string>(),
                    Record = new ConcurrentDictionary<string, string>(),
                    Errors = new ConcurrentDictionary<string, string>()
                } } }
            };

            failedRowsDictionary4.ConcatenateData("FourthKey", null);

            Assert.AreEqual(1, failedRowsDictionary4.Count);
            Assert.AreEqual(1, failedRowsDictionary4["FourthKey"].Count);



            var failedRowsDictionary3 = new Dictionary<string, IList<FailedRecord>>();

            failedRowsDictionary3.ConcatenateData("ThirdKey", null);

            Assert.AreEqual(1, failedRowsDictionary3.Count);
            Assert.AreEqual(0, failedRowsDictionary3["ThirdKey"].Count);
        }

        [TestMethod]
        public void ConcatenateData_Two_Different_Keys()
        {
            var failedRowsDictionary5 = new Dictionary<string, IList<FailedRecord>>
            {
                {"FirstKey", new List<FailedRecord> {new FailedRecord
                {
                    CustomData = new ConcurrentDictionary<string, string>(),
                    Record = new ConcurrentDictionary<string, string>(),
                    Errors = new ConcurrentDictionary<string, string>()
                } } }
            };

            failedRowsDictionary5.ConcatenateData("FourthKey", new List<FailedRecord>
            {
                new FailedRecord
                {
                    CustomData = new ConcurrentDictionary<string, string>()
                }
            });

            Assert.AreEqual(2, failedRowsDictionary5.Count);
            Assert.AreEqual(1, failedRowsDictionary5["FourthKey"].Count);
            Assert.AreEqual(1, failedRowsDictionary5["FirstKey"].Count);
        }

        [TestMethod]
        public void ConcatenateData_InvalidKey()
        {
            var failedRowsDictionary6 = new Dictionary<string, IList<FailedRecord>>
            {
                {"FirstKey", new List<FailedRecord> {new FailedRecord
                {
                    CustomData = new ConcurrentDictionary<string, string>(),
                    Record = new ConcurrentDictionary<string, string>(),
                    Errors = new ConcurrentDictionary<string, string>()
                } } }
            };

            failedRowsDictionary6.ConcatenateData("", new List<FailedRecord>
            {
                new FailedRecord
                {
                    CustomData = new ConcurrentDictionary<string, string>()
                }
            });

            Assert.AreEqual(1, failedRowsDictionary6.Count);
            Assert.AreEqual(1, failedRowsDictionary6["FirstKey"].Count);

        }

        [TestMethod]
        public void GetEndpointsWithVerbForUpsertTest()
        {
            IReadOnlyDictionary<HtmlVerb, string> endPoints1 = new Dictionary<HtmlVerb, string>
            {
                { HtmlVerb.Post, string.Empty},
                {HtmlVerb.Put, string.Empty}
            };

            var result1 = endPoints1.GetEndpointsWithVerbForUpsert();
            Assert.AreEqual(result1.Value, string.Empty);

            IReadOnlyDictionary<HtmlVerb, string> endPoints2 = new Dictionary<HtmlVerb, string>
            {
                { HtmlVerb.Post, "localhost/employee/{employeeId}/employees"},
                {HtmlVerb.Put, "localhost/employee/{employeeId}/employees/{id}"}
            };

            var result2 = endPoints2.GetEndpointsWithVerbForUpsert();
            Assert.AreEqual(result2.Value, string.Empty);

            IReadOnlyDictionary<HtmlVerb, string> endPoints3 = new Dictionary<HtmlVerb, string>
            {
                { HtmlVerb.Post, "localhost/employeetax/taxes"},
                {HtmlVerb.Put, "localhost/employeetax/taxes/{id}"}
            };

            var result3 = endPoints3.GetEndpointsWithVerbForUpsert();
            Assert.AreEqual(result3.Value, "localhost/employeetax/taxes");
            Assert.AreEqual(result3.Key, HtmlVerb.Post);

            IReadOnlyDictionary<HtmlVerb, string> endPoints4 = new Dictionary<HtmlVerb, string>
            {
                { HtmlVerb.Post, "localhost/employeetax/taxes"},
                {HtmlVerb.Put, "localhost/employeetax/taxes/12345"},
                {HtmlVerb.Patch, "localhost/employeetax/taxes/12345"}
            };

            var result4 = endPoints4.GetEndpointsWithVerbForUpsert();
            Assert.AreEqual(result4.Value, "localhost/employeetax/taxes/12345");
            Assert.AreEqual(result4.Key, HtmlVerb.Patch);
        }
    }
}
