using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Extensions
{
    [TestClass]
    public class ClientMappingExtensionsTest
    {
        [TestClass]
        public class UpdateGeneratedMappingNameTests
        {

            private readonly ClientMapping[] _clientMappings = JsonConvert.DeserializeObject<ClientMapping[]>(File.ReadAllText("json\\ClientMappingTests.json"));
            
            [TestMethod]
            public void VerifyEmptyListDoesntFail()
            {
                var testList = new List<ClientMapping>();
                testList.UpdateGeneratedMappingName();
                Assert.IsTrue(true);
            }

            [TestMethod]
            public void VerifyGeneratedMappingNameGenerated()
            {
                var mappingToTest = _clientMappings[0];
                mappingToTest.UpdateGeneratedMappingName();
                Assert.AreEqual(mappingToTest.MappingName, mappingToTest.GeneratedMappingName);
            }
        }
    }
}
