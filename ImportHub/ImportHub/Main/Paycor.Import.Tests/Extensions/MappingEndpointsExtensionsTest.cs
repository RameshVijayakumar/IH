using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappingEndpointsExtensionsTest
    {
        [TestMethod]
        public void GetListOfAllMappingEndpointsTest()
        {
            var mappingEndpoints = new MappingEndpoints
            {
                Post = "localhost/employeeservice/v2/employees",
                Put = "localhost/employeeservice/v2/employees/{id}",
                Patch = null,
                Delete = null
            };

            var result = mappingEndpoints.GetListOfAllMappingEndpoints();
            Assert.AreEqual("localhost/employeeservice/v2/employees", result.ElementAt(0));
            Assert.AreEqual("localhost/employeeservice/v2/employees/{id}", result.ElementAt(1));
        }

        [TestMethod]
        public void GetListOfAllNoMappingEndpointsTest()
        {
            var mappingEndpoints = new MappingEndpoints
            {
                Post = null,
                Put = null,
                Patch = null,
                Delete = null
            };

            var result = mappingEndpoints.GetListOfAllMappingEndpoints();
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetListOfAllMappingEndpointsNullTest()
        {
            var mappingEndpoints = new MappingEndpoints();
            mappingEndpoints = null;
            

            var result = mappingEndpoints.GetListOfAllMappingEndpoints();
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetListOfOptedOutEndPointsInfoTest()
        {
            var mappingEndpoints = new MappingEndpoints
            {
                Post = "localhost/employeeservice/v2/employees",
                Put = "localhost/employeeservice/v2/employees/{id}",
                Patch = null,
                Delete = null
            };

            var result = mappingEndpoints.GetListOfOptedOutEndPointsInfo();
            Assert.AreEqual("Patch", result[0]);
            Assert.AreEqual("Delete", result[1]);
        }

        [TestMethod]
        public void GetListOfOptedOutEndPointsNullInfoTest()
        {
            var mappingEndpoints = new MappingEndpoints();
            mappingEndpoints = null;

            var result = mappingEndpoints.GetListOfOptedOutEndPointsInfo();
            Assert.AreEqual(0, result.Count);
        }
    }
}
