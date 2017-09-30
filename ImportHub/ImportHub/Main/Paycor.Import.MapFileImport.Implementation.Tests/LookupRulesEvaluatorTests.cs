using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using log4net;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [TestClass]
    public class LookupRulesEvaluatorTests
    {
        [TestMethod]
        public void SortLookupOrder_Has_Dependencies_And_NoDependencies()
        {
            var lookupEvalutor = new LookupRulesEvaluator(new Mock<ILog>().Object);

            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("expectedStartDate", "08012016"),
                new KeyValuePair<string, string>("reason", "fun"),
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "47"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345")
            };
            var employeeMapping =
                JsonConvert.DeserializeObject<ApiMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));

            var sortedData = lookupEvalutor.SortLookupOrder(data, employeeMapping.Mapping, new FieldDefinitionComparer());

            Assert.AreEqual(5, sortedData.Count());
            Assert.AreEqual(
                "http://localhost/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}",
                sortedData.ElementAt(0).EndPoint);
            Assert.AreEqual(
                "http://localhost/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}",
                sortedData.ElementAt(1).EndPoint);
            Assert.AreEqual(
                "http://localhost/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}",
                sortedData.ElementAt(2).EndPoint);
            Assert.AreEqual(
                "http://localhost/leavemanagementservice/v1/employees/{employeeid}/leaveCases?reason={{reason}}&expectedStartDate={{expectedStartDate}}",
                sortedData.ElementAt(3).EndPoint);
            Assert.AreEqual(
                "http://localhost/employeeservice/v2/employees?employeeid={{employeeid}}&expectedStartDate={{expectedStartDate}}&leavecaseid={{leavecaseid}}",
                sortedData.ElementAt(4).EndPoint);

            Assert.AreEqual("{dummyemployeeid}", sortedData.ElementAt(0).Destination);
            Assert.AreEqual("{dummyemployeeid}", sortedData.ElementAt(1).Destination);
            Assert.AreEqual("{employeeid}", sortedData.ElementAt(2).Destination);
            Assert.AreEqual("{leavecaseid}", sortedData.ElementAt(3).Destination);
            Assert.AreEqual("{employeecaseid}", sortedData.ElementAt(4).Destination);

            var recordColumnsValues = data.ToList();
            foreach (var mappingFieldDefinition in sortedData)
            {
                if (!lookupEvalutor.ValidateRules(mappingFieldDefinition, data,
                    recordColumnsValues.Select(t => t.Key).ToList().Distinct()
                )) break;
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

    }
}
