
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;

namespace Paycor.Import.Status.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BaseStatusReceiverTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        public class TestStatusReceiver : BaseStatusReceiver<string>
        {
            public TestStatusReceiver(string reporter) : base(reporter)
            {
            }

            public TestStatusReceiver()
            {
            }

            protected override string Serialize(string statusType)
            {
                return statusType + "_serialized";
            }
        }

        [TestClass]
        public class SendTests
        {
            private readonly string _key = "myKey";
            private readonly string _status = "myStatus";
            private readonly string _reporter = "myReporter";
            private readonly string _classReporter = "classReporter";

            [TestMethod]
            public void Send_Calls_OnSend()
            {
                var receiver = new TestStatusReceiver();
                receiver.StatusReceived += Receiver_StatusReceived;
                receiver.Send(_key, _status, _reporter);
            }

            private void Receiver_StatusReceived(object sender, StatusMessage e)
            {
                Assert.AreEqual(_key, e.Key);
                Assert.AreEqual(_status + "_serialized", e.Status);
                Assert.AreEqual(_reporter, e.Reporter);
            }

            [TestMethod]
            public void Send_Calls_OnSend_WithClassReporter()
            {
                var receiver = new TestStatusReceiver(_classReporter);
                receiver.StatusReceived += Receiver_StatusReceived_ClassReporter;
                receiver.Send(_key, _status);
            }

            private void Receiver_StatusReceived_ClassReporter(object sender, StatusMessage e)
            {
                Assert.AreEqual(_key, e.Key);
                Assert.AreEqual(_status + "_serialized", e.Status);
                Assert.AreEqual(_classReporter, e.Reporter);
            }
        }

        [TestClass]
        public class CTorTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CTor_Enforces_Reporter()
            {
                var val = new TestStatusReceiver(null);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentException))]
            public void CTor_Enforces_Reporter_string()
            {
                var val = new TestStatusReceiver(string.Empty);
            }
        }
    }
}
