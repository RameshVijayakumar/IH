using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Paycor.Import.Mapping;
using Paycor.Import.Http;
using log4net;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Transformers;
using Paycor.Import.Messaging;


namespace Paycor.Import.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappingFieldDefinitionRecordTransformerTest
    {

        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        Guid _masterSessionId = new Guid("484e8e9e-b1f5-4864-8ed3-6b0fcbe284a9");

        [TestMethod]
        public void Source_Transformer_For_Valid_Data()
        {
            const string firstName = "Alan";
            const string lastName = "Shepard";
            const string dateOfFlight = "05/05/1961";
            var source = new Dictionary<string, string>
            {
                {"First", firstName},
                {"Last", lastName},
                {"Date", dateOfFlight},
            };

            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "First Name",
                        Source = "First",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Last Name",
                        Source = "Last",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Date of Flight",
                        Source = "Date"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Last Name2",
                        Source = "{Name}"
                    }
                }
            };

            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(
                t =>
                    t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HtmlVerb>(),
                        null));
            var ilog = new Mock<ILog>();
          
            var rulesEvaluator = new Mock<IRulesEvaluator>();
            var lookupResolver = new Mock<ILookupResolver<MappingFieldDefinition>>();
            var transformers = new List<ITransformRecordFields<MappingDefinition>>
            {
                new SourceFieldTransformer(),
                new LookupRouteFieldTransformer(ilog.Object,
                lookupResolver.Object, rulesEvaluator.Object)
                
            };
            var kvpList = new List<List<KeyValuePair<string, string>>>();
            var lookup = new Mock<ILookup>();
            foreach (var transformer in transformers)
            {
                var result = transformer.TransformRecordFields(mappingDefinition,
                    _masterSessionId.ToString(), source, null, lookup.Object).ToList();
                kvpList.Add(result);
            }

            Assert.AreEqual(firstName, kvpList[0].Single(t => t.Key == "First Name").Value);
            Assert.AreEqual(lastName, kvpList[0].Single(t => t.Key == "Last Name").Value);
            Assert.AreEqual(dateOfFlight, kvpList[0].Single(t => t.Key == "Date of Flight").Value);
        }

        
        [TestMethod]
        public void Source_Transformer_For_Zero_Mapping_FieldDef()
        {
            const string firstName = "Alan";
            const string lastName = "Shepard";
            const string dateOfFlight = "05/05/1961";
            var source = new Dictionary<string, string>
            {
                {"First", firstName},
                {"Last",  lastName},
                {"Date",  dateOfFlight},
            };

            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition()
                }
            };


              var transformers = new List<ITransformRecordFields<MappingDefinition>>
            {
                new SourceFieldTransformer()
            }
            ;

            List<KeyValuePair<string, string>> kvpRecord = null;
            var kvpList = new List<List<KeyValuePair<string, string>>>();
            var lookup = new Mock<ILookup>();
            foreach (var transformer in transformers)
            {
                var result = transformer.TransformRecordFields(mappingDefinition,
                    _masterSessionId.ToString(), source, kvpRecord, lookup.Object).ToList();
                kvpList.Add(result);
            Assert.AreEqual(0, result.Count);
        }
        }

        
        [TestMethod]
        public void Source_Transformer_For_Invalid_Mapping_FieldDef()
        {
            const string firstName = "Alan";
            const string lastName = "Shepard";
            const string dateOfFlight = "05/05/1961";
            var source = new Dictionary<string, string>
            {
                {"First", firstName},
                {"Last", lastName},
                {"Date", dateOfFlight},
            };

            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "First Name",
                        Source = "Firt",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Last Name",
                        Source = "Lat",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Date of Flight",
                        Source = "Dte"
                    }
                }
            };

              var transformers = new List<ITransformRecordFields<MappingDefinition>>
            {
                new SourceFieldTransformer()
            };

            List<KeyValuePair<string, string>> kvpRecord = null;
            var kvpList = new List<List<KeyValuePair<string, string>>>();
            var lookup = new Mock<ILookup>();
            foreach (var transformer in transformers)
            {
                var result = transformer.TransformRecordFields(mappingDefinition,
                    _masterSessionId.ToString(), source, kvpRecord, lookup.Object).ToList();
                kvpList.Add(result);
            Assert.AreEqual(0, result.Count);
            }
        }

        
        [TestMethod]
        public void Source_Transformer_Returns_Zero_Results_For_Mismatch_Source_Fields()
        {
            const string firstName = "Alan";
            const string lastName = "Shepard";
            const string dateOfFlight = "05/05/1961";
            var source = new Dictionary<string, string>
            {
                {"First", firstName},
                {"Last", lastName},
                {"Date", dateOfFlight},
            };

            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "First Name",
                        Source = "Firt",
                        Required = true
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Last Name",
                        Source = "Lat",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Date of Flight",
                        Source = "Dte"
                    }
                }
            };

             var transformers = new List<ITransformRecordFields<MappingDefinition>>
            {
                new SourceFieldTransformer()
            };

            List<KeyValuePair<string, string>> kvpRecord = null;
            var kvpList = new List<List<KeyValuePair<string, string>>>();
            var lookup = new Mock<ILookup>();
            foreach (var transformer in transformers)
            {
                var result = transformer.TransformRecordFields(mappingDefinition,
                    _masterSessionId.ToString(), source, kvpRecord, lookup.Object).ToList();
                kvpList.Add(result);
                Assert.AreEqual(0, result.Count);
        }
        }

        
        [TestMethod]
        public void Transformers_For_Zero_Mapping_FieldDef()
        {
            const string firstName = "Alan";
            const string lastName = "Shepard";
            const string dateOfFlight = "05/05/1961";
            var source = new Dictionary<string, string>
            {
                {"First", firstName},
                {"Last", lastName},
                {"Date", dateOfFlight},
            };

            var mappingDefinition = new MappingDefinition
            {
            };
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HtmlVerb>(), null));
            var Ilog = new Mock<ILog>();
            var rulesEvaluator = new Mock<IRulesEvaluator>();
            var lookupResolver = new Mock<ILookupResolver<MappingFieldDefinition>>();
            var transformers = new List<ITransformRecordFields<MappingDefinition>>
            {
                new SourceFieldTransformer(),
                 new LookupRouteFieldTransformer(Ilog.Object, lookupResolver.Object,
                 rulesEvaluator.Object
                 )

            };

            List<KeyValuePair<string, string>> kvpRecord = null;
            var kvpList = new List<List<KeyValuePair<string, string>>>();
            var lookup = new Mock<ILookup>();
            foreach (var transformer in transformers)
            {
                var result = transformer.TransformRecordFields(mappingDefinition,
                    _masterSessionId.ToString(), source, kvpRecord, lookup.Object).ToList();
                kvpList.Add(result);
            Assert.AreEqual(0, result.Count);
        }
        }

        
        [TestMethod]
        public void LookupRoute_Transformer_With_No_LookupRecord()
        {
            const string firstName = "Alan";
            const string lastName = "Shepard";
            const string dateOfFlight = "05/05/1961";
            var source = new Dictionary<string, string>
            {
                {"First", firstName},
                {"Last", lastName},
                {"Date", dateOfFlight},
            };

            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        EndPoint = "localhost:54911/import/referenceapi/v2/formula1/drivers?firstName={firstName}",
                        Destination = "DriverId",
                        Source = "{First}",
                    },
                    new MappingFieldDefinition
                    {
                        EndPoint = "localhost:54911/import/referenceapi/v2/formula1/drivers?lastName={LastName}",
                        Destination = "DriverId",
                        Source = "{Last}",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Date of Flight",
                        Source = "Date"
                    }
                }
            };



            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HtmlVerb>(), null));
             var ilog = new Mock<ILog>();
            var rulesEvaluator = new Mock<IRulesEvaluator>();
            var lookupResolver = new Mock<ILookupResolver<MappingFieldDefinition>>();
            var transformers = new List<ITransformRecordFields<MappingDefinition>>
            {
                new SourceFieldTransformer(),
                new LookupRouteFieldTransformer(ilog.Object, lookupResolver.Object,
                 rulesEvaluator.Object
                 )

            };

            List<KeyValuePair<string, string>> kvpRecord = null;
            var kvpList = new List<List<KeyValuePair<string, string>>>();
            var lookup = new Mock<ILookup>();
            foreach (var transformer in transformers)
            {
                var result = transformer.TransformRecordFields(mappingDefinition,
                    _masterSessionId.ToString(), source, kvpRecord, lookup.Object).ToList();
                kvpList.Add(result);
            }

            Assert.AreEqual(dateOfFlight, kvpList[0].Single(t => t.Key == "Date of Flight").Value);
        }


        [TestMethod]
        public void Source_And_LookupRoute_Transformer_For_Valid_Data()
        {
            const string firstName = "Alan";
            const string lastName = "Shepard";
            const string dateOfFlight = "05/05/1961";
            var source = new Dictionary<string, string>
            {
                {"First", firstName},
                {"Last", lastName},
                {"Date", dateOfFlight},
            };

            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        EndPoint = "localhost:54911/import/referenceapi/v2/formula1/drivers?firstName={firstName}",
                        Destination = "DriverId",
                        Source = "{First}",
                    },
                    new MappingFieldDefinition
                    {
                        EndPoint = "localhost:54911/import/referenceapi/v2/formula1/drivers?lastName={LastName}",
                        Destination = "DriverId",
                        Source = "{Last}",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Date of Flight",
                        Source = "Date"
                    }
                }
            };


            var response = JToken.Parse(File.ReadAllText("Json\\MockWebApiResponse.json"));
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var httpExporterResult = new HttpExporterResult
            {
                Response = httpResponseMessage,
                
            };
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HtmlVerb>(), null))
                .Returns(httpExporterResult);
             var ilog = new Mock<ILog>();
            var rulesEvaluator = new Mock<IRulesEvaluator>();
            var lookupResolver = new Mock<ILookupResolver<MappingFieldDefinition>>();
            var transformers = new List<ITransformRecordFields<MappingDefinition>>
            {
                new SourceFieldTransformer(),
                 new LookupRouteFieldTransformer(ilog.Object, lookupResolver.Object,
                 rulesEvaluator.Object
                 )

            };

            var kvpRecord = source.Select(t=>new KeyValuePair<string,string>(t.Key,t.Value)).ToList();
            var kvpList = new List<List<KeyValuePair<string, string>>>();
            var lookup = new Mock<ILookup>();
            foreach (var transformer in transformers)
            {
                var result = transformer.TransformRecordFields(mappingDefinition,
                    _masterSessionId.ToString(), source, kvpRecord, lookup.Object).ToList();
                kvpList.Add(result);
            }

            Assert.AreEqual(dateOfFlight, kvpList[0].Single(t => t.Key == "Date of Flight").Value);
        }

        
        [TestMethod]
        public void LookupRoute_Transformer_For_Valid_Data_Gives_ExceptionMessage()
        {
            const string firstName = "Alan";
            const string lastName = "Shepard";
            const string dateOfFlight = "05/05/1961";
            var source = new Dictionary<string, string>
            {
                {"First", firstName},
                {"Last", lastName},
                {"Date", dateOfFlight},
            };

            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "First",
                        Source = "First",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Last",
                        Source = "Last",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "Date of Flight",
                        Source = "Date"
                    },
                    new MappingFieldDefinition
                    {
                        EndPoint = "localhost:54911/import/referenceapi/v2/formula1/drivers?lastName={LastName}",
                        Destination = "DriverId",
                        Source = "{Last}",
                        ExceptionMessage = "Driver not found"
                    },
                }
            };
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HtmlVerb>(), null));
             var ilog = new Mock<ILog>();
            var rulesEvaluator = new Mock<IRulesEvaluator>();
            var lookupResolver = new Mock<ILookupResolver<MappingFieldDefinition>>();

             rulesEvaluator.Setup(t => t.ValidateRules(
                 It.IsAny<MappingFieldDefinition>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                 It.IsAny<IEnumerable<string>>())).Returns(true);
             rulesEvaluator.Setup(t => t.SortLookupOrder(
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                 It.IsAny<MappingDefinition>(),
                 It.IsAny<IEqualityComparer<MappingFieldDefinition>>())).Returns(
                 new List<MappingFieldDefinition>()
                 {
                     new MappingFieldDefinition
                     {
                         EndPoint = "localhost:54911/import/referenceapi/v2/formula1/drivers?lastName={LastName}",
                         Destination = "DriverId",
                         Source = "{Last}",
                         ExceptionMessage = "Driver not found"
                     }
                 });

            

            var transformers = new List<ITransformRecordFields<MappingDefinition>>
            {
                new SourceFieldTransformer(),
                  new LookupRouteFieldTransformer(ilog.Object, lookupResolver.Object,
                 rulesEvaluator.Object
                 )
            };

            var kvpRecord = source.Select(t => new KeyValuePair<string, string>(t.Key, t.Value)).ToList();
            var kvpList = new List<List<KeyValuePair<string, string>>>();
            var lookup = new Mock<ILookup>();
            foreach (var transformer in transformers)
            {
                var result = transformer.TransformRecordFields(mappingDefinition,
                    _masterSessionId.ToString(), source, kvpRecord, lookup.Object).ToList();
                kvpList.Add(result);
            }
            Assert.AreEqual("Driver not found", kvpList[1].Single(t => t.Key == ImportConstants.LookUpRouteExceptionMessageKey).Value);
        }
    }

}
