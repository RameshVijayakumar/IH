using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappingFieldDefinitionExtensionsTest
    {

        [TestMethod]
        public void GetSubArrayFieldsOfThreeInstances_Test_SubArrayObjects()
        {
            const string refKey = "address";
            const string propertyName = "street";
            var field = new MappingFieldDefinition
            {
                Destination = $"{refKey}[].{propertyName}",
                Source = $"{refKey} {propertyName}",
                Type = "string",
                GlobalLookupType = null,
                Required = false
            };

            var result = field.GetSubArrayFieldsOfMultipleInstances(3, refKey, propertyName);
            var mappingFieldDefinitions = result as MappingFieldDefinition[] ?? result.ToArray();
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(1).Destination, "address[0].street");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(2).Destination, "address[1].street");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(3).Destination, "address[2].street");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(1).Source, "address street1");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(2).Source, "address street2");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(3).Source, "address street3");
        }

        [TestMethod]
        public void GetSubArrayFieldsOfThreeInstances_Test_StringSubArray()
        {
            const string refKey = "address";
            var field = new MappingFieldDefinition
            {
                Destination = $"{refKey}[]",
                Source = refKey,
                Type = "string",
                GlobalLookupType = null,
                Required = false
            };

            var result = field.GetSubArrayFieldsOfMultipleInstances(3, refKey);
            var mappingFieldDefinitions = result as MappingFieldDefinition[] ?? result.ToArray();
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(1).Destination, "address[0]");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(2).Destination, "address[1]");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(3).Destination, "address[2]");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(1).Source, "address1");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(2).Source, "address2");
            Assert.AreEqual(mappingFieldDefinitions.ElementAt(3).Source, "address3");
        }

        [TestMethod]
        public void GetSourceIgnoringPipe()
        {
            var mapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiConcatenationMapping.json"));

            var result = mapping.Mapping.FieldDefinitions.First(t=>t.Source== "firstName|lastName").GetSourceIgnoringPipe();
            Assert.AreEqual("firstName", result);
        }

        [TestMethod]
        public void GetConcatenationFields()
        {
            var mapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiConcatenationMapping.json"));

            var result =
                mapping.Mapping.GetConcatenationFields(
                    mapping.Mapping.FieldDefinitions.First(t => t.Source == "firstName|lastName"));
            Assert.AreEqual("person.firstName", result.First());
            Assert.AreEqual("person.lastName", result.ElementAt(1));
        }
    }
}
