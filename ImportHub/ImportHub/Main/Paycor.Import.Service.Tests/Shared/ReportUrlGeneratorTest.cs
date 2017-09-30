using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Service.Shared;

namespace Paycor.Import.Service.Tests.Shared
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ReportUrlGeneratorTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void GetReportUrl_ValidClient()
        {
            var clientId = "66670";
            var importFileName = "TestImport";
            var expectedReportUrl = "http://localhost/Documents//api/documents/parameterizedreport/TestImport?ClientID=66670&OutputType=PDF&AccentColor=0&Environment=Development";
            var reportUrlGenerator = new ReportUrlGenerator();

            var privateObject = new PrivateObject(reportUrlGenerator);
            var actualReportUrl = privateObject.Invoke("GetReportUrl", clientId, importFileName);

            Assert.IsNotNull(actualReportUrl);
            Assert.AreEqual(expectedReportUrl, actualReportUrl);
        }

        [TestMethod]
        public void GetReportUrl_InvalidClient()
        {
            var clientId = "0";
            var importFileName = "TestImport";
            var expectedReportUrl = "http://localhost/Documents//api/documents/parameterizedreport/TestImport?OutputType=PDF&AccentColor=0&Environment=Development";
            var reportUrlGenerator = new ReportUrlGenerator();

            var privateObject = new PrivateObject(reportUrlGenerator);
            var actualReportUrl = privateObject.Invoke("GetReportUrl", clientId, importFileName);

            Assert.IsNotNull(actualReportUrl);
            Assert.AreEqual(expectedReportUrl, actualReportUrl);
        }
    }
}