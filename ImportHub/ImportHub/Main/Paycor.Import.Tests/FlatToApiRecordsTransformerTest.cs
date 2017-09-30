using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Transformers;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FlatToApiRecordsTransformerTest
    {
        [TestMethod]
        public void TranslateFlatRecordsToSubArrays_SimpleSubArrayObject()
        {
            var mapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\SimpleSubArrayMapping.json"));

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

                var expected = new List<ApiRecord>()
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
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
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
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
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
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
                }
            };

            var calculate = new Mock<ICalculate>();
            var subArrayTransformer = new FlatToApiRecordsTransformer(calculate.Object);
            var actual = subArrayTransformer.TranslateFlatRecordsToApiRecords(records,
                mapping, new ImportContext());

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
            var mapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\StringArrayMapping.json"));
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

            var calculate = new Mock<ICalculate>();
            var subArrayTransformer = new FlatToApiRecordsTransformer(calculate.Object);
            var expected = new List<ApiRecord>()
            {
                new ApiRecord
                {
                    Record = new Dictionary<string, string> { {"name","john" } },

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

            var actual = subArrayTransformer.TranslateFlatRecordsToApiRecords
                (records, mapping, new ImportContext());

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
            var mapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\StringAndSubArrayMapping.json"));
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

            var calculate = new Mock<ICalculate>();
            var subArrayTransformer = new FlatToApiRecordsTransformer(calculate.Object);
            var actual = subArrayTransformer.TranslateFlatRecordsToApiRecords(records, 
                mapping,new ImportContext());

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

