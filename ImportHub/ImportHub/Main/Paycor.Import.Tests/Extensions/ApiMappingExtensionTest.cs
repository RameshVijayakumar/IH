using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApiMappingExtensionTest
    {
        [TestMethod]
        public void GetAllRequiredFieldsTest()
        {
            var apiMapping = new GeneratedMapping
            {
                MappingName = "TestReqFields",
                Mapping = new MappingDefinition
                {
                    FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Source = "LastName",
                            Required = true
                        },
                        new MappingFieldDefinition
                        {
                            Source = "MiddleName",
                            Required = false
                        },
                        new MappingFieldDefinition
                        {
                            Source = "FirstName",
                            Required = true
                        }
                    }
                    
                }
            };

            var result = apiMapping.GetAllRequiredFields();
            Assert.AreEqual("LastName", result[0]);
            Assert.AreEqual("FirstName", result[1]);
        }

        [TestMethod]
        public void GetAllRequiredFieldsNullTest()
        {
            var apiMapping = new GeneratedMapping
            {
                MappingName = "TestReqFields",
                Mapping = new MappingDefinition
                {
                    FieldDefinitions = new List<MappingFieldDefinition>()

                }
            };

            var result = apiMapping.GetAllRequiredFields();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetAllRequiredFieldsFieldDefNullTest()
        {
            var apiMapping = new GeneratedMapping();

            var result = apiMapping.GetAllRequiredFields();
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void GetLookupEndpointsWithoutQueryStringTest()
        {
            var apiMapping = new GeneratedMapping
            {
                MappingName = "TestReqFields",
                Mapping = new MappingDefinition
                {
                    FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                            Required = true,
                            EndPoint = "http://localhost/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "{{clientId}}&clientCode={{clientCode}}",
                            Required = false,
                            EndPoint = "http://localhost/abcservice/v2/earnings?clientId={{clientId}}&clientCode={{clientCode}}"
                        },
                        new MappingFieldDefinition
                        {
                            Source = "FirstName",
                            Required = true,
                            EndPoint = null
                        }
                    }
                }
            };

            var result = apiMapping.GetLookupEndpointsWithoutQueryString();
            Assert.AreEqual("http://localhost/employeeservice/v2/employees", result[0]);
            Assert.AreEqual("http://localhost/abcservice/v2/earnings", result[1]);
        }

         [TestMethod]
        public void DoesEndPointsOtherThanPostExist_Test()
        {
            var mapping = new GeneratedMapping
            {
                MappingEndpoints = new MappingEndpoints()
                {
                    Delete = null,
                    Patch = null,
                    Post = null,
                    Put = null
                }
            };
            Assert.AreEqual(false, mapping.IsEndPointOtherThanPost());

            var mapping1 = new GeneratedMapping
            {
                MappingEndpoints = new MappingEndpoints()
                {
                    Delete = "",
                    Patch = "sad",
                    Post = "sdsds",
                    Put = null
                }
            };

            Assert.AreEqual(true, mapping1.IsEndPointOtherThanPost());
        }
    }
}
