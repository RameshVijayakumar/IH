using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Extensions
{
    [TestClass]
    public class GeneratedMappingExtensionsTest
    {
        [TestMethod]
        public void RemoveActionColTest()
        {
            var apiMapping = new GeneratedMapping
            {
                MappingName = "TestActionColRemoval",
                Mapping = new MappingDefinition
                {
                    FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                            Destination = "{id}",
                            Required = true,
                            EndPoint = "http://localhost/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "{{clientId}}&clientCode={{clientCode}}",
                            Destination = "{id}",
                            Required = false,
                            EndPoint = "http://localhost/abcservice/v2/earnings?clientId={{clientId}}&clientCode={{clientCode}}"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "FirstName",
                            Destination = "FirstName",
                            Required = true,
                            EndPoint = null
                        },
                        new MappingFieldDefinition
                        {
                            Source = "action",
                            Destination = "ih:action",
                            Required = false
                        }
                    }
                }
            };

            var result = apiMapping.RemoveActionFieldFromMap();
            Assert.AreEqual(result.Mapping.FieldDefinitions.Any(t => t.Destination != ImportConstants.ActionFieldName), true);
        }

    }
}
