using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Implementation.Shared;
using Paycor.Import.MapFileImport.Implementation.Transformers;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FlatToApiRecordsTransformerTest
    {
        private readonly Calculate _calculate = new Calculate();

        [TestMethod]
        public void TestApiRecordMissingPayload()
        {
            var apiMapping = new GeneratedMapping
            {
                MappingName = "Client Report",

                Mapping = new MappingDefinition
                {
                    FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Destination = "reportedByClientId",
                            Source = "reportedByClientId"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "reportedByEmployeeNumber",
                            Source = "reportedByEmployeeNumber"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "reviewedByClientId",
                            Source = "reviewedByClientId"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "reviewedByEmployeeNumber",
                            Source = "reviewedByEmployeeNumber"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "clientId",
                            Source = "clientId"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "employeeNumber",
                            Source = "employeeNumber"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "{reportedByEmployeeId}",
                            Source = "{{reportedByClientId}}&employeeNumber={{reportedByEmployeeNumber}}",
                            IsRequiredForPayload = true
                        }

                    }
                }
            };  

            var records = new List<IDictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    ["reportedByClientId"] = "102",
                    ["reportedByEmployeeNumber"] = "22137",
                    ["reviewedByClientId"] = "102",
                    ["reviewedByEmployeeNumber"] = "22137",
                    ["employeeNumber"] = "100016",
                    ["clientId"] = "22137"
                },
                new Dictionary<string, string>
                {
                    ["reportedByClientId"] = "102",
                    ["reportedByEmployeeNumber"] = "22137",
                    ["reviewedByClientId"] = "102",
                    ["reviewedByEmployeeNumber"] = "22137",
                    ["employeeNumber"] = "100016",
                    ["clientId"] = "22137",
                    ["reportedByEmployeeId"] = "000-000-000-000"
                }
            };

            var context = new ImportContext
            {
                ChunkNumber = 1,
                ChunkSize = 1000
            };

            var subArrayTransformer = new FlatToApiRecordsTransformer(_calculate);
            var actual = subArrayTransformer.TranslateFlatRecordsToApiRecords(records, apiMapping, context);
            
            Assert.AreEqual(true, actual[0].IsPayloadMissing);
            Assert.AreEqual(false, actual[1].IsPayloadMissing);
        }


        [TestMethod]
        public void TranslateFlatRecordsToSubArrays_SimpleSubArrayObject()
        {
            var apiMapping = new GeneratedMapping
            {
                MappingName = "Address SubArray",

                Mapping = new MappingDefinition
                {
                    FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Destination = "name",
                            Source = "name"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "addresses[0].country",
                            Source = "HomeCountry"
                        }
                    }
                }
            };


            var records = new List<IDictionary<string, string>>
            {

                new Dictionary<string, string>
                {
                    ["name"] = "john",
                    ["addresses[0].country"] = "usa",
                },
                new Dictionary<string, string>
                {
                    ["name"] = "david",
                    ["addresses[0].country"] = "france",
                },
                new Dictionary<string, string>
                {
                    ["name"] = "joseph",
                    ["addresses[0].country"] = "germany",
                }

            };

            var expected = new List<ApiRecord>
            {
                new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","john" } },
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "country","usa"}}
                            },
                            ArrayName = "addresses"
                        }
                    },
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>(),
                    RowNumber = 1,
                    IsPayloadMissing = false
                },
                      new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","david" } },
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "country","france"}}
                            },
                            ArrayName = "addresses"
                        }
                    },
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>(),
                    RowNumber = 2,
                    IsPayloadMissing = false
                },
                     new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","joseph" } },
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "country","germany"}}
                            },
                            ArrayName = "addresses"
                        }
                    },
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>(),
                    RowNumber = 3,
                    IsPayloadMissing = false
                }
            };

            var context = new ImportContext
            {
                ChunkNumber = 1,
                ChunkSize = 1000
            };

            var subArrayTransformer = new FlatToApiRecordsTransformer(_calculate);
            var actual = subArrayTransformer.TranslateFlatRecordsToApiRecords(records, apiMapping, context);

            Assert.AreEqual(expected.Count, actual.Count);

            Assert.IsTrue(expected[0].Record.All(e => actual[0].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[0].ApiPayloadStringArrays, actual[0].ApiPayloadStringArrays);
            Assert.AreEqual(expected[0].ApiPayloadArrays.Count, actual[0].ApiPayloadArrays.Count);
            Assert.IsTrue(expected[0].ApiPayloadArrays[0].ArrayData.ToArray()[0].All(e => actual[0].ApiPayloadArrays[0].ArrayData.ToArray()[0].Contains(e)));
            Assert.AreEqual(expected[0].ApiPayloadArrays[0].ArrayName, actual[0].ApiPayloadArrays[0].ArrayName);

            Assert.IsTrue(expected[1].Record.All(e => actual[1].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[1].ApiPayloadStringArrays, actual[1].ApiPayloadStringArrays);
            Assert.AreEqual(expected[1].ApiPayloadArrays.Count, actual[1].ApiPayloadArrays.Count);
            Assert.IsTrue(expected[1].ApiPayloadArrays[0].ArrayData.ToArray()[0].All(e => actual[1].ApiPayloadArrays[0].ArrayData.ToArray()[0].Contains(e)));
            Assert.AreEqual(expected[1].ApiPayloadArrays[0].ArrayName, actual[1].ApiPayloadArrays[0].ArrayName);

            Assert.IsTrue(expected[2].Record.All(e => actual[2].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[2].ApiPayloadStringArrays, actual[2].ApiPayloadStringArrays);
            Assert.AreEqual(expected[2].ApiPayloadArrays.Count, actual[2].ApiPayloadArrays.Count);
            Assert.IsTrue(expected[2].ApiPayloadArrays[0].ArrayData.ToArray()[0].All(e => actual[2].ApiPayloadArrays[0].ArrayData.ToArray()[0].Contains(e)));
            Assert.AreEqual(expected[2].ApiPayloadArrays[0].ArrayName, actual[2].ApiPayloadArrays[0].ArrayName);

        }


        [TestMethod]
        public void TranslateFlatRecordsToSubArrays_StringArray()
        {
            var apiMapping = new GeneratedMapping
            {
                MappingName = "Address",

                Mapping = new MappingDefinition
                {
                    FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Destination = "name",
                            Source = "name"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "cities[0]",
                            Source = "livingcity",
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "cities[1]",
                            Source = "workcity",
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "cities[2]",
                            Source = "interestedcity",
                        }
                    }
                }
            };
            var records = new List<IDictionary<string, string>>
            {

                new Dictionary<string, string>
                {
                    ["name"] = "john",
                    ["cities[0]"] = "dallas",
                    ["cities[1]"] = "frisco",
                    ["cities[2]"] = "cinci",
                },
                new Dictionary<string, string>
                {
                    ["name"] = "david",
                    ["cities[0]"] = "paris",
                    ["cities[1]"] = "london",
                    ["cities[2]"] = "berlin",
                },
                new Dictionary<string, string>
                {
                    ["name"] = "rajni",
                    ["cities[0]"] = "chennai",
                    ["cities[1]"] = "beijing",
                    ["cities[2]"] = "hanoi",

                }

            };
            
            var subArrayTransformer = new FlatToApiRecordsTransformer(_calculate);
            var expected = new List<ApiRecord>()
            {
                new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","john" } },
                    RowNumber = 1,
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>() {  new ApiPayloadStringArray() { StringArrayName = "cities", StringArrayData = new List<string>() { "dallas","frisco","cinci" } } },
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>()
                        }
                    },

                },
                      new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","david" } },
                    RowNumber = 2,
                    IsPayloadMissing = false,
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>()
                        }
                    },
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>() {  new ApiPayloadStringArray() { StringArrayName = "cities",  StringArrayData = new List<string>() { "paris","london","berlin" } } },

                },
                     new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","rajni" } },
                    RowNumber = 3,
                    IsPayloadMissing = false,
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>()
                        }
                    },
                     ApiPayloadStringArrays = new List<ApiPayloadStringArray>() {  new ApiPayloadStringArray() { StringArrayName = "cities", StringArrayData = new List<string>() { "chennai", "beijing", "hanoi" } } },

                }
            };

            var context = new ImportContext
            {
                ChunkNumber = 1,
                ChunkSize = 1000
            };
            var actual = subArrayTransformer.TranslateFlatRecordsToApiRecords(records, apiMapping, context);

            Assert.AreEqual(expected.Count, actual.Count);

            Assert.IsTrue(expected[0].Record.All(e => actual[0].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[0].ApiPayloadStringArrays[0].StringArrayData, actual[0].ApiPayloadStringArrays[0].StringArrayData);
            Assert.AreEqual(expected[0].ApiPayloadStringArrays[0].StringArrayName, actual[0].ApiPayloadStringArrays[0].StringArrayName);

            Assert.IsTrue(expected[1].Record.All(e => actual[1].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[1].ApiPayloadStringArrays[0].StringArrayData, actual[1].ApiPayloadStringArrays[0].StringArrayData);
            Assert.AreEqual(expected[1].ApiPayloadStringArrays[0].StringArrayName, actual[1].ApiPayloadStringArrays[0].StringArrayName);

            Assert.IsTrue(expected[2].Record.All(e => actual[2].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[2].ApiPayloadStringArrays[0].StringArrayData, actual[2].ApiPayloadStringArrays[0].StringArrayData);
            Assert.AreEqual(expected[2].ApiPayloadStringArrays[0].StringArrayName, actual[2].ApiPayloadStringArrays[0].StringArrayName);


        }

        [TestMethod]
        public void TranslateFlatRecordsToSubArrays_StringArray_And_SubArrayObject()
        {
            var apiMapping = new GeneratedMapping
            {
                MappingName = "Address SubArray",

                Mapping = new MappingDefinition
                {

                    FieldDefinitions = new List<MappingFieldDefinition>
                    {
                        new MappingFieldDefinition
                        {
                            Destination = "name",
                            Source = "name"
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "cities[0]",
                            Source = "livingcity",
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "cities[1]",
                            Source = "workcity",
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "cities[2]",
                            Source = "interestedcity",
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "addresses[0].country",
                            Source = "HomeCountry",
                        },
                        new MappingFieldDefinition
                        {
                            Destination = "addresses[1].country",
                            Source = "WorkCountry",
                        }
                    }
                }
            };
            var records = new List<IDictionary<string, string>>
            {

                new Dictionary<string, string>
                {
                    ["name"] = "john",
                    ["cities[0]"] = "dallas",
                    ["cities[1]"] = "frisco",
                    ["cities[2]"] = "cinci",
                    ["addresses[0].country"] = "usa",
                    ["addresses[1].country"] = "germany",
                },
                new Dictionary<string, string>
                {
                    ["name"] = "david",
                    ["cities[0]"] = "paris",
                    ["cities[1]"] = "london",
                    ["cities[2]"] = "berlin",
                    ["addresses[0].country"] = "france",
                    ["addresses[1].country"] = "china"
                },
                new Dictionary<string, string>
                {
                    ["name"] = "rajni",
                    ["cities[0]"] = "chennai",
                    ["cities[1]"] = "beijing",
                    ["cities[2]"] = "hanoi",
                    ["addresses[0].country"] = "india",
                    ["addresses[1].country"] = "denmark"

                }

            };

            var expected = new List<ApiRecord>()
            {
                new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","john" } },
                    RowNumber = 1,
                    IsPayloadMissing = false,
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>() {  new ApiPayloadStringArray() { StringArrayName = "cities", StringArrayData = new List<string>() { "dallas","frisco","cinci" } } },
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "country","usa"}},
                                new Dictionary<string, string> { { "country", "germany" } }
                            },
                            ArrayName = "addresses"
                        }

                    }

                },
                      new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","david" } },
                    RowNumber = 2,
                    IsPayloadMissing = false,
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                      new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "country","france"}},
                                new Dictionary<string, string> { { "country", "china" } }
                            },
                            ArrayName = "addresses"
                        },

                    },
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>() {  new ApiPayloadStringArray() { StringArrayName = "cities", StringArrayData = new List<string>() { "paris","london","berlin" } } },

                },
                     new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","rajni" } },
                    RowNumber = 3,
                    IsPayloadMissing = false,
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                      new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "country","india"}},
                                new Dictionary<string, string> { { "country", "denmark" } }

                            },
                            ArrayName = "addresses"
                        },
                    },
                     ApiPayloadStringArrays = new List<ApiPayloadStringArray>() {  new ApiPayloadStringArray() { StringArrayName = "cities", StringArrayData = new List<string>() { "chennai", "beijing", "hanoi" } } },

                }
            };

            var context = new ImportContext
            {
                ChunkNumber = 1,
                ChunkSize = 1000
            };

            var subArrayTransformer = new FlatToApiRecordsTransformer(_calculate);
            var actual = subArrayTransformer.TranslateFlatRecordsToApiRecords(records, apiMapping, context);

            Assert.AreEqual(expected.Count, actual.Count);

            Assert.IsTrue(expected[0].Record.All(e => actual[0].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[0].ApiPayloadStringArrays[0].StringArrayData, actual[0].ApiPayloadStringArrays[0].StringArrayData);
            Assert.AreEqual(expected[0].ApiPayloadStringArrays[0].StringArrayName, actual[0].ApiPayloadStringArrays[0].StringArrayName);


            Assert.IsTrue(expected[0].ApiPayloadArrays[0].ArrayData.ToArray()[0].All(e => actual[0].ApiPayloadArrays[0].ArrayData.ToArray()[0].Contains(e)));
            Assert.IsTrue(expected[0].ApiPayloadArrays[0].ArrayData.ToArray()[1].All(e => actual[0].ApiPayloadArrays[0].ArrayData.ToArray()[1].Contains(e)));

            Assert.IsTrue(expected[1].Record.All(e => actual[1].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[1].ApiPayloadStringArrays[0].StringArrayData, actual[1].ApiPayloadStringArrays[0].StringArrayData);
            Assert.AreEqual(expected[1].ApiPayloadStringArrays[0].StringArrayName, actual[1].ApiPayloadStringArrays[0].StringArrayName);

            Assert.IsTrue(expected[1].ApiPayloadArrays[0].ArrayData.ToArray()[0].All(e => actual[1].ApiPayloadArrays[0].ArrayData.ToArray()[0].Contains(e)));
            Assert.IsTrue(expected[1].ApiPayloadArrays[0].ArrayData.ToArray()[1].All(e => actual[1].ApiPayloadArrays[0].ArrayData.ToArray()[1].Contains(e)));


            Assert.IsTrue(expected[2].Record.All(e => actual[2].Record.Contains(e)));
            CollectionAssert.AreEqual(expected[2].ApiPayloadStringArrays[0].StringArrayData, actual[2].ApiPayloadStringArrays[0].StringArrayData);
            Assert.AreEqual(expected[2].ApiPayloadStringArrays[0].StringArrayName, actual[2].ApiPayloadStringArrays[0].StringArrayName);
            Assert.IsTrue(expected[2].ApiPayloadArrays[0].ArrayData.ToArray()[0].All(e => actual[2].ApiPayloadArrays[0].ArrayData.ToArray()[0].Contains(e)));
            Assert.IsTrue(expected[2].ApiPayloadArrays[0].ArrayData.ToArray()[1].All(e => actual[2].ApiPayloadArrays[0].ArrayData.ToArray()[1].Contains(e)));

        }
    }
}

