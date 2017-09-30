
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;

namespace Paycor.Import.Status.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class StatusManagerTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestClass]
        public class CTorTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Enforces_StatusReceiver()
            {
                // ReSharper disable once UnusedVariable
                var x = new StatusManager<string>(null, new Mock<IStatusStorageProvider>().Object);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Enforces_StorageProvider()
            {
                // ReSharper disable once UnusedVariable
                var x = new StatusManager<string>(new Mock<IStatusReceiver<string>>().Object, null);
            }
        }
    }
}
