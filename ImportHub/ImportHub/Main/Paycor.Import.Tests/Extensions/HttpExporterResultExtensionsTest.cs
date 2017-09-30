using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.Http;

namespace Paycor.Import.Tests.Extensions
{
    [TestClass]
    public class HttpExporterResultExtensionsTest
    {
        [TestMethod]
        public void GetLinkFromApi_Null_Test()
        {
            var output = ((HttpExporterResult) null).GetLinkFromApi();
            Assert.AreEqual(output, null);
        }
    }
}
