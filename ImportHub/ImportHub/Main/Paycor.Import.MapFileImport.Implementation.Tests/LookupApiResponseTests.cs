using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class LookupApiResponseTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }
        [TestMethod]
        public void Lookup_Store()
        {
            var lookup = new LookupApiResponse();

            const string data = "I stored the response";
            const string key = "http://localhost/employeeservice";

            const string newData = "new response";

            lookup.Store(key, data);

            Assert.AreEqual(lookup.Retrieve(key),data);

            lookup.Store(key,newData);

            Assert.AreEqual(lookup.Retrieve(key), newData);

            lookup.Store(string.Empty, newData);

            Assert.AreEqual(lookup.Retrieve(string.Empty), newData);

            lookup.Store(string.Empty, string.Empty);


            Assert.AreEqual(lookup.Retrieve(string.Empty), string.Empty);
        }

        [TestMethod]
        public void Lookup_Remove()
        {
            var lookup = new LookupApiResponse();

            const string data = "I stored the response";
            const string key = "http://localhost/employeeservice";

            lookup.Store(key, data);
            lookup.Remove(key);

            Assert.AreEqual(lookup.Retrieve(key),null);

            lookup.Remove(key);
        }
    }
}
