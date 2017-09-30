using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.Mapping;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.Tests.Mapping
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApiMappingTraverserTests
    {
        private static List<GeneratedMapping> _sampleMappings;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            using (StreamReader r = new StreamReader("Mapping\\MappingResponse.json"))
            {
                string json = r.ReadToEnd();
                _sampleMappings = JsonConvert.DeserializeObject<List<GeneratedMapping>>(json);
            }
        }

        [TestMethod]
        public void AllRouteParametersSatisfiedByFields()
        {
            List<string> fields = new List<string>()
            {
                "clientId",
                "departmentNumber"
            };

            var results = ApiMappingTraverser.GetBestMatchedMappings(_sampleMappings, fields)
                .Select(m => m.MappingName);

            CollectionAssert.DoesNotContain(results.ToList(), "MyLeaveCase2");
        }

        [TestMethod]
        public void AllRouteParametersSatisfiedByFields_TestFieldCase()
        {
            List<string> fields = new List<string>()
            {
                "employeeId",
                "departmentNumber"
            };

            var results = ApiMappingTraverser.GetBestMatchedMappings(_sampleMappings, fields)
                .Select(m => m.MappingName);

            CollectionAssert.Contains(results.ToList(), "MyLeaveCase2");
            CollectionAssert.DoesNotContain(results.ToList(), "Alias1");
        }

        [TestMethod]
        public void AllRouteParametersSatisfiedByFields_TestFieldSpace()
        {
            List<string> fields = new List<string>()
            {
                "employee Id",
                "departmentNumber"
            };

            var results = ApiMappingTraverser.GetBestMatchedMappings(_sampleMappings, fields)
                .Select(m => m.MappingName);

            CollectionAssert.Contains(results.ToList(), "MyLeaveCase2");
            CollectionAssert.DoesNotContain(results.ToList(), "Alias1");
        }

        [TestMethod]
        public void AllRouteParametersSatisfiedByFields2()
        {
            List<string> fields = new List<string>()
            {
                "employeeid",
                "departmentNumber"
            };

            var results = ApiMappingTraverser.GetBestMatchedMappings(_sampleMappings, fields)
                .Select(m => m.MappingName);

            CollectionAssert.Contains(results.ToList(), "MyLeaveCase2");
            CollectionAssert.DoesNotContain(results.ToList(), "Alias1");
        }

        [TestMethod]
        public void GetMappingsThatFullyCoverFields()
        {
            List<string> fields = new List<string>()
            {
                "clientId",
                "type",
                "key",
                "value"
            };

            var results = ApiMappingTraverser.GetBestMatchedMappings(_sampleMappings, fields);
            var exactMatches = ApiMappingTraverser.GetMappingsThatFullyCoverColumns(results, fields);

            Assert.IsTrue(exactMatches.All(m => m.MappingName == "Alias1"), "Additional Maps Returned");
            Assert.IsTrue(exactMatches.Any(), "No Maps Returned");

        }

        [TestMethod]
        public void GetMappingsThatFullyCoverFields2()
        {
            List<string> fields = new List<string>()
            {
                "client Id",
                "type",
                "key",
                "value"
            };

            var results = ApiMappingTraverser.GetBestMatchedMappings(_sampleMappings, fields);
            var exactMatches = ApiMappingTraverser.GetMappingsThatFullyCoverColumns(results, fields);

            Assert.IsTrue(exactMatches.All(m => m.MappingName == "Alias1"), "Additional Maps Returned");
            Assert.IsTrue(exactMatches.Any(), "No Maps Returned");
        }

        [TestMethod]
        public void GetMappingsThatFullyCoverFields3()
        {
            List<string> fields = new List<string>()
            {
                "client Id",
                "employee Number"
            };

            var results = ApiMappingTraverser.GetBestMatchedMappings(_sampleMappings, fields);
            var exactMatches = ApiMappingTraverser.GetMappingsThatFullyCoverColumns(results, fields);

            Assert.IsTrue(exactMatches.All(m => m.MappingName == "MyLeaveCase2"), "Additional Maps Returned");
            Assert.IsTrue(exactMatches.Any(), "No Maps Returned");
        }
    }
}
