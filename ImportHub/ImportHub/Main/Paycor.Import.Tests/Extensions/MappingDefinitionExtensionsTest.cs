using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Extensions
{
    [TestClass]
    public class MappingDefinitionExtensionsTest
    {
        [TestMethod]
        public void GetAllSourceFieldsWithoutLookupAndConst_Test()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {

                    new MappingFieldDefinition
                    {
                        Source = "id",
                        Destination = "id",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientId",
                        Destination = "clientId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeNumber",
                        Destination = "employeeNumber",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "code",
                        Destination = "code",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "sourceType",
                        Destination = "sourceType",
                        SourceType = SourceTypeEnum.Const
                    },
                    new MappingFieldDefinition
                    {
                        Source = "effectiveStartDate",
                        Destination = "effectiveStartDate",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeId",
                        Destination = "employeeId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientDeductionId",
                        Destination = "clientDeductionId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                        Destination = "{employeeId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&clientCode={{clientCode}}",
                        Destination = "{clientDeductionId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{clientCode}",
                        Destination = "{id}",
                        SourceType = SourceTypeEnum.File
                    }
                }
            };

            var listOfSource = mappingDefinition.GetAllSourceFieldsWithoutLookupAndConst().ToList();
            Assert.AreEqual(7, listOfSource.Count);
        }

        [TestMethod]
        public void GetHeaderFieldsFromMappingFields_Test()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {

                    new MappingFieldDefinition
                    {
                        Source = "id",
                        Destination = "id",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientId",
                        Destination = "clientId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeNumber",
                        Destination = "employeeNumber",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "code",
                        Destination = "code",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "sourceType",
                        Destination = "sourceType",
                        SourceType = SourceTypeEnum.Const
                    },
                    new MappingFieldDefinition
                    {
                        Source = "effectiveStartDate",
                        Destination = "effectiveStartDate",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeId",
                        Destination = "employeeId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientDeductionId",
                        Destination = "clientDeductionId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                        Destination = "{employeeId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&clientCode={{clientCode}}",
                        Destination = "{clientDeductionId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{clientCode}",
                        Destination = "{id}",
                        SourceType = SourceTypeEnum.File
                    }
                }
            };

            var listOfHeaders = mappingDefinition.GetHeaderFieldsFromMappingFields().ToList();
            Assert.AreEqual(4, listOfHeaders.Count);
        }

        [TestMethod]
        public void GetHeaderFieldsFromMappingFields_WithSubArrays_Test()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {

                    new MappingFieldDefinition
                    {
                        Source = "id",
                        Destination = "id",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientId",
                        Destination = "clientId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeNumber",
                        Destination = "employeeNumber",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "code",
                        Destination = "code",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "sourceType",
                        Destination = "sourceType",
                        SourceType = SourceTypeEnum.Const
                    },
                    new MappingFieldDefinition
                    {
                        Source = "effectiveStartDate",
                        Destination = "effectiveStartDate",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeId",
                        Destination = "employeeId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientDeductionId",
                        Destination = "clientDeductionId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                        Destination = "{employeeId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&clientCode={{clientCode}}",
                        Destination = "{clientDeductionId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{clientCode}",
                        Destination = "{id}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[].country",
                        Source = "addresses[].country"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[0].state",
                        Source = "addresses[0].state"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[1].zip",
                        Source = "addresses[1].zip"
                    }
                }
            };

            var listOfHeaders = mappingDefinition.GetHeaderFieldsFromMappingFields().ToList();
            Assert.AreEqual(6, listOfHeaders.Count);
        }

        [TestMethod]
        public void GetAllDestinationFieldsWhichAreLookups_Test()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {

                    new MappingFieldDefinition
                    {
                        Source = "id",
                        Destination = "id",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientId",
                        Destination = "clientId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeNumber",
                        Destination = "employeeNumber",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "code",
                        Destination = "code",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "sourceType",
                        Destination = "sourceType",
                        SourceType = SourceTypeEnum.Const
                    },
                    new MappingFieldDefinition
                    {
                        Source = "effectiveStartDate",
                        Destination = "effectiveStartDate",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeId",
                        Destination = "employeeId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientDeductionId",
                        Destination = "clientDeductionId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                        Destination = "{employeeId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&clientCode={{clientCode}}",
                        Destination = "{clientDeductionId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{clientCode}",
                        Destination = "{id}",
                        SourceType = SourceTypeEnum.File
                    }
                }
            };

            var listOfLookupDest = mappingDefinition.GetAllDestinationFieldsWhichAreLookups().ToList();
            Assert.AreEqual(3, listOfLookupDest.Count);
        }

        [TestMethod]
        public void TestGetAllSourceFieldsWithGenericSubArray_NoGeneric()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[0].country",
                        Source = "Address1"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[1].country",
                        Source = "Address2"
                    }
                }
            };

            var list = mappingDefinition.GetAllSourceFieldsWithGenericSubArray();
            Assert.AreEqual(0, list.Count());
        }

        [TestMethod]
        public void TestGetAllSourceFieldsWithGenericSubArray_OneGeneric()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[].country",
                        Source = "Address"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[0].state",
                        Source = "Address1"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[1].zip",
                        Source = "Address2"
                    }
                }
            };

            var list = mappingDefinition.GetAllSourceFieldsWithGenericSubArray();
            Assert.AreEqual(1, list.Count());
        }

        [TestMethod]
        public void TestGetAllSourceFieldsWithGenericSubArray_AllGeneric()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[].country",
                        Source = "Country1"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[].state",
                        Source = "State1"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[]",
                        Source = "Address"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "addresses[].zip",
                        Source = "Zip1"
                    }
                }
            };

            var list = mappingDefinition.GetAllSourceFieldsWithGenericSubArray();
            Assert.AreEqual(4, list.Count());
        }

        [TestMethod]
        public void GetHeaderFieldsFromMappingFieldsWithNoAction_Test()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {

                    new MappingFieldDefinition
                    {
                        Source = "id",
                        Destination = "id",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientId",
                        Destination = "clientId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeNumber",
                        Destination = "employeeNumber",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "code",
                        Destination = "code",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "sourceType",
                        Destination = "sourceType",
                        SourceType = SourceTypeEnum.Const
                    },
                    new MappingFieldDefinition
                    {
                        Source = "effectiveStartDate",
                        Destination = "effectiveStartDate",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeId",
                        Destination = "employeeId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientDeductionId",
                        Destination = "clientDeductionId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                        Destination = "{employeeId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&clientCode={{clientCode}}",
                        Destination = "{clientDeductionId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{clientCode}",
                        Destination = "{id}",
                        SourceType = SourceTypeEnum.File
                    }
                }
            };

            var listOfHeaders = mappingDefinition.GetHeaderFieldsFromMappingFieldsWithActionOnTop().ToList();
            Assert.AreEqual(4, listOfHeaders.Count);
            Assert.AreEqual("clientId", listOfHeaders[0]);
        }

        [TestMethod]
        public void GetHeaderFieldsFromMappingFieldsWithActionOnTop_Test()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {

                    new MappingFieldDefinition
                    {
                        Source = "id",
                        Destination = "id",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientId",
                        Destination = "clientId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeNumber",
                        Destination = "employeeNumber",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "code",
                        Destination = "code",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "sourceType",
                        Destination = "sourceType",
                        SourceType = SourceTypeEnum.Const
                    },
                    new MappingFieldDefinition
                    {
                        Source = "effectiveStartDate",
                        Destination = "effectiveStartDate",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeId",
                        Destination = "employeeId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "action",
                        Destination = "ih:action",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                        Destination = "{employeeId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&clientCode={{clientCode}}",
                        Destination = "{clientDeductionId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{clientCode}",
                        Destination = "{id}",
                        SourceType = SourceTypeEnum.File
                    }
                }
            };

            var listOfHeaders = mappingDefinition.GetHeaderFieldsFromMappingFieldsWithActionOnTop().ToList();
            Assert.AreEqual(5, listOfHeaders.Count);
            Assert.AreEqual("action", listOfHeaders[0]);
        }

        [TestMethod]
        public void GetAllLookupSourceFieldsTest()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Source = "clientId",
                        Destination = "clientId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeNumber",
                        Destination = "employeeNumber",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "code",
                        Destination = "code",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "sourceType",
                        Destination = "sourceType",
                        SourceType = SourceTypeEnum.Const
                    },
                    new MappingFieldDefinition
                    {
                        Source = "effectiveStartDate",
                        Destination = "effectiveStartDate",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "employeeId",
                        Destination = "employeeId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "clientDeductionId",
                        Destination = "clientDeductionId",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&employeeNumber={{employeeNumber}}",
                        Destination = "{employeeId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{{clientId}}&clientCode={{clientCode}}",
                        Destination = "{clientDeductionId}",
                        SourceType = SourceTypeEnum.File
                    },
                    new MappingFieldDefinition
                    {
                        Source = "{clientCode}",
                        Destination = "{id}",
                        SourceType = SourceTypeEnum.File
                    }
                }
            };

            var result = mappingDefinition.GetAllLookupSourceFields();
            Assert.AreEqual("clientId", result.ElementAt(0));
            Assert.AreEqual("employeeNumber", result.ElementAt(1));
            Assert.AreEqual("clientCode", result.ElementAt(2));
        }
    }
}
