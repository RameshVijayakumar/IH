using Moq;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;

namespace Paycor.Import.Status.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BaseStatusRetrieverTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        class TestStatusRetriever : BaseStatusRetriever<string>
        {
            public TestStatusRetriever(IStatusStorageProvider provider, string reporter) 
                : base(provider, reporter)
            {

            }

            protected override string Deserialize(StatusMessage message)
            {
                return message.Status + "_deserialized";
            }
        }

        [TestClass]
        public class CTorTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CTor_Enforces_StatusStorageProvider()
            {
                var x = new TestStatusRetriever(null, "bogus");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CTor_Enforces_Reporter()
            {
                var x = new TestStatusRetriever(new Mock<IStatusStorageProvider>().Object, null);
            }
        }

        [TestClass]
        public class RetrievStatusTests
        {
            [TestMethod]
            public void RetrieveStatus_Calls_Deserialize()
            {
                var mockStorageProvider = new Mock<IStatusStorageProvider>();
                mockStorageProvider.Setup(
                    f => f.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>((reporter, key) => new StatusMessage
                    {
                        Reporter = reporter,
                        Key = key,
                        Status = "Test Status"
                    });

                var retriever = new TestStatusRetriever(mockStorageProvider.Object, "MyReporter");
                var status = retriever.RetrieveStatus("myKey");

                Assert.AreEqual("Test Status_deserialized", status);
            }
        }
    }
}
