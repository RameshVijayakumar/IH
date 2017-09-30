using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Mapping
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class NestedDictionaryConverterTests
    {
        [TestMethod]
        public void Convert_TopLevel()
        {
            IDictionary<string, string> dict = new Dictionary<string, string>
            {
                ["id"] = "123",
                ["employeeId"] = "123456789",
                ["clientId"] = "102"
            };

            var actual = JsonConvert.SerializeObject(dict, new NestedDictionaryConverter());
            var expected = "{\"clientId\":\"102\",\"employeeId\":\"123456789\",\"id\":\"123\"}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Convert_Nested_OneLevel()
        {
            IDictionary<string, string> dict = new Dictionary<string, string>
            {
                ["id"] = "123",
                ["person.id"] = "123",
                ["person.ssn"] = "123-45-6789",
                ["employeeId"] = "123456789",
                ["person.lastName"] = "Smith",
                ["person.firstName"] = "John",
                ["manager.id"] = "456",
                ["manager.lastName"] = "Didley",
                ["manager.firstName"] = "Bo",
                ["clientId"] = "102"
            };

            var actual = JsonConvert.SerializeObject(dict, new NestedDictionaryConverter());
            var expected = "{\"clientId\":\"102\",\"employeeId\":\"123456789\",\"id\":\"123\"," +
                "\"manager\":{\"firstName\":\"Bo\",\"id\":\"456\",\"lastName\":\"Didley\"}," +
                "\"person\":{\"firstName\":\"John\",\"id\":\"123\",\"lastName\":\"Smith\",\"ssn\":\"123-45-6789\"}}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Convert_Nested_MultiLevel()
        {
            IDictionary<string, string> dict = new Dictionary<string, string>
            {
                ["id"] = "123",
                ["person.id"] = "123",
                ["person.ssn"] = "123-45-6789",
                ["employeeId"] = "123456789",
                ["person.name.last"] = "Smith",
                ["person.name.first"] = "John",
                ["person.name.middle"] = "J",
                ["clientId"] = "102"
            };
            var actual = JsonConvert.SerializeObject(dict, new NestedDictionaryConverter());
            var expected = "{\"clientId\":\"102\",\"employeeId\":\"123456789\",\"id\":\"123\"," + 
                "\"person\":{\"id\":\"123\",\"ssn\":\"123-45-6789\"," + 
                "\"name\":{\"first\":\"John\",\"last\":\"Smith\",\"middle\":\"J\"}}}";
            Assert.AreEqual(expected, actual);
        }
    }
}
