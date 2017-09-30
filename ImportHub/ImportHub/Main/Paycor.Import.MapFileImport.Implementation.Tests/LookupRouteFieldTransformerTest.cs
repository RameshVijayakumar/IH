using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Implementation.Transformers;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using LookupRouteFieldTransformer = Paycor.Import.MapFileImport.Implementation.Transformers.LookupRouteFieldTransformer;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class LookupRouteFieldTransformerTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void One_Lookup_Success_Test()
        {
            var ilog = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(),It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string,string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                    }
                
                });

            var lookupRulesEvaluator = new LookupRulesEvaluator(ilog.Object);
            var lookupResolver = new LookupResolver(ilog.Object, httpInvoker.Object, 
                new RouteParameterFormatter());
            var transformer = new LookupRouteFieldTransformer(ilog.Object,lookupResolver, lookupRulesEvaluator);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var transformedData = transformer.TransformRecordFields(employeeMapping.Mapping, 
                Guid.NewGuid().ToString(),
                new Dictionary<string, string>(),
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("clientId","Ad?D"),
                    new KeyValuePair<string, string>("employeeNumber","(47&)"),
                    new KeyValuePair<string, string>("WorkPhone","(232-232-2345)")
                }).ToList();

            Assert.AreEqual(5,transformedData.Count);

            var employeeIdValue = transformedData
                .FirstOrDefault(t => t.Key == "employeeid");
                
            Assert.AreEqual("0e68fec2-0630-0000-0000-000066000000", employeeIdValue.Value);

        }

        [TestMethod]
        public void One_Lookup_NotFound_BCOS_Of_InvalidResponse_Test()
        {
            var ilog = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage()
                

                });

            var lookupRulesEvaluator = new LookupRulesEvaluator(ilog.Object);
            var lookupResolver = new LookupResolver(ilog.Object, httpInvoker.Object, 
                new RouteParameterFormatter());
            var transformer = new LookupRouteFieldTransformer(ilog.Object, lookupResolver,lookupRulesEvaluator);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var transformedData = transformer.TransformRecordFields(employeeMapping.Mapping,
                Guid.NewGuid().ToString(),
                new Dictionary<string, string>(),
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("clientId","65273"),
                    new KeyValuePair<string, string>("employeeNumber","477"),
                    new KeyValuePair<string, string>("WorkPhone","232-232-2345")
                }).ToList();

            Assert.AreEqual(4, transformedData.Count);

            var exceptionMessage = transformedData
                .FirstOrDefault(t => t.Key == "LookupRoute.ExceptionMessage");

            Assert.AreEqual("Employee Not Found (clientId:65273,employeeNumber:477)\nEmployee Not Found (clientId:65273,employeeNumber:477)\nEmployee Not Found (clientId:65273,employeeNumber:477)", 
                    exceptionMessage.Value);

        }

        [TestMethod]
        public void One_Lookup_NotFound_BCOS_of_EmptyResponse_Test()
        {
            var ilog = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent("[]",Encoding.UTF8, "application/json")
                    }

                });
            var lookupRulesEvaluator = new LookupRulesEvaluator(ilog.Object);
            var lookupResolver = new LookupResolver(ilog.Object, httpInvoker.Object, 
                new RouteParameterFormatter());
            var transformer = new LookupRouteFieldTransformer(ilog.Object, lookupResolver,lookupRulesEvaluator);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var transformedData = transformer.TransformRecordFields(employeeMapping.Mapping,
                Guid.NewGuid().ToString(),
                new Dictionary<string, string>(),
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("expectedStartDate","08012016"),
                    new KeyValuePair<string, string>("reason","fun"),
                    new KeyValuePair<string, string>("clientId","65273"),
                    new KeyValuePair<string, string>("employeeNumber","47"),
                    new KeyValuePair<string, string>("WorkPhone","232-232-2345"),
                }).ToList();

            Assert.AreEqual(6, transformedData.Count);

            var exceptionMessage = transformedData
                .FirstOrDefault(t => t.Key == "LookupRoute.ExceptionMessage");

            Assert.AreEqual("Employee Not Found (clientId:65273,employeeNumber:47)\nEmployee Not Found (clientId:65273,employeeNumber:47)\nEmployee Not Found (clientId:65273,employeeNumber:47)\nCould not find employee for clientId and employee number (reason:fun,expectedStartDate:08012016)", 
                            exceptionMessage.Value);

        }

        [TestMethod]
        public void Three_Lookup_Success_Test()
        {
            var ilog = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                    }

                });

            var lookupRulesEvaluator = new LookupRulesEvaluator(ilog.Object);
            var lookupResolver = new LookupResolver(ilog.Object, httpInvoker.Object,
                new RouteParameterFormatter());
            var transformer = new LookupRouteFieldTransformer(ilog.Object, lookupResolver,lookupRulesEvaluator);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var transformedData = transformer.TransformRecordFields(employeeMapping.Mapping,
                Guid.NewGuid().ToString(),
                new Dictionary<string, string>(),
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("expectedStartDate","08012016"),
                    new KeyValuePair<string, string>("reason","fun"),
                    new KeyValuePair<string, string>("clientId","65273"),
                    new KeyValuePair<string, string>("employeeNumber","47"),
                    new KeyValuePair<string, string>("WorkPhone","232-232-2345"),
                    
                    
                }).ToList();

            Assert.AreEqual(9, transformedData.Count);

            var employeeIdValue = transformedData
                .FirstOrDefault(t => t.Key == "employeeid");

            Assert.AreEqual("0e68fec2-0630-0000-0000-000066000000", employeeIdValue.Value);
            Assert.AreEqual("0e68fec2-0630-0000-0000-000066000000",
                transformedData.FirstOrDefault(t=>t.Key == "leavecaseid").Value);
            Assert.AreEqual("0e68fec2-0630-0000-0000-000066000000",
                transformedData.FirstOrDefault(t=>t.Key == "employeecaseid").Value);

        }


        [TestMethod]
        public void Sort_Lookup_Test()
        {
            var ilog = new Mock<ILog>();
            var lookupRuleEngine = new LookupRulesEvaluator(ilog.Object);

            var data = new List<KeyValuePair<string, string>>
            {
                 new KeyValuePair<string, string>("expectedStartDate","08012016"),
                 new KeyValuePair<string, string>("reason","fun"),
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "47"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345")
            };
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));

            var sortedData = lookupRuleEngine.SortLookupOrder(data, employeeMapping.Mapping, new FieldDefinitionComparer());

            Assert.AreEqual(5,sortedData.Count());
            //            Debug.WriteLine(sortedData.ElementAt(0).EndPoint);
            //            Debug.WriteLine(sortedData.ElementAt(1).EndPoint);
            //            Debug.WriteLine(sortedData.ElementAt(2).EndPoint);
            //            Debug.WriteLine(sortedData.ElementAt(3).EndPoint);
            //            Debug.WriteLine(sortedData.ElementAt(4).EndPoint);

            Debug.WriteLine(sortedData.ElementAt(0).Destination);
            Debug.WriteLine(sortedData.ElementAt(1).Destination);
            Debug.WriteLine(sortedData.ElementAt(2).Destination);
            Debug.WriteLine(sortedData.ElementAt(3).Destination);
            Debug.WriteLine(sortedData.ElementAt(4).Destination);
        }


        [TestMethod]
        public void ONE_URL_ToManyDest_Lookup_Success_Test()
        {
            var ilog = new Mock<ILog>();
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\1URLToManyDestLookupMapping.json"));

            var lookupRulesEvaluator = new LookupRulesEvaluator(ilog.Object);
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                    }

                });
            var lookupResolver = new LookupResolver(ilog.Object, httpInvoker.Object, 
                new RouteParameterFormatter());
            var transformer = new LookupRouteFieldTransformer(ilog.Object, lookupResolver, lookupRulesEvaluator);

            var transformedData = transformer.TransformRecordFields(employeeMapping.Mapping,
                Guid.NewGuid().ToString(),
                new Dictionary<string, string>(),
                new List<KeyValuePair<string, string>>
                {
                       new KeyValuePair<string, string>("expectedStartDate","08012016"),
                 new KeyValuePair<string, string>("reason","fun"),
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "47"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("reportedByClientId", "100273"),
                new KeyValuePair<string, string>("reportedByEmployeeNumber", "252"),
                new KeyValuePair<string, string>("reviewedByClientId", "100273"),
                new KeyValuePair<string, string>("reviewedByEmployeeNumber", "252")


                }).ToList();


            Assert.AreEqual(12, transformedData.Count);
        }


        [TestMethod]
        public void Many_URL_ToOneDest_Lookup_Success_Test()
        {
            var ilog = new Mock<ILog>();
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\ManyURLToSameDest.json"));

            var lookupRulesEvaluator = new LookupRulesEvaluator(ilog.Object);
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                    }

                });
            var lookupResolver = new LookupResolver(ilog.Object, httpInvoker.Object,
                new RouteParameterFormatter());
            var transformer = new LookupRouteFieldTransformer(ilog.Object, lookupResolver, lookupRulesEvaluator);

            var transformedData = transformer.TransformRecordFields(employeeMapping.Mapping,
                Guid.NewGuid().ToString(),
                new Dictionary<string, string>(),
                new List<KeyValuePair<string, string>>
                {
                       new KeyValuePair<string, string>("expectedStartDate","08012016"),
                 new KeyValuePair<string, string>("reason","fun"),
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "47"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("reportedByClientId", "100273"),
                new KeyValuePair<string, string>("reportedByEmployeeNumber", "252"),
                new KeyValuePair<string, string>("reviewedByClientId", "100273"),
                new KeyValuePair<string, string>("reviewedByEmployeeNumber", "252")


                }).ToList();


            Assert.AreEqual(11, transformedData.Count);
        }

        [TestMethod]
        public void Many_URL_ToOneDest_Lookup_Fail_Test()
        {
            var ilog = new Mock<ILog>();
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\ManyURLToSameDest.json"));

            var lookupRulesEvaluator = new LookupRulesEvaluator(ilog.Object);
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("Json\\EmptyLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                    }

                });
            var lookupResolver = new LookupResolver(ilog.Object, httpInvoker.Object,
                new RouteParameterFormatter());
            var transformer = new LookupRouteFieldTransformer(ilog.Object, lookupResolver, lookupRulesEvaluator);

            var transformedData = transformer.TransformRecordFields(employeeMapping.Mapping,
                Guid.NewGuid().ToString(),
                new Dictionary<string, string>(),
                new List<KeyValuePair<string, string>>
                {
                       new KeyValuePair<string, string>("expectedStartDate","08012016"),
                 new KeyValuePair<string, string>("reason","fun"),
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", "47"),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345"),
                new KeyValuePair<string, string>("reportedByClientId", "  "),
                new KeyValuePair<string, string>("reportedByEmployeeNumber", ""),
                new KeyValuePair<string, string>("reviewedByClientId", "100273"),
                new KeyValuePair<string, string>("reviewedByEmployeeNumber", "252")


                }).ToList();


            Assert.AreEqual(10, transformedData.Count);
        }

        [TestMethod]
        public void Sort_Lookup_MissingLookupField_Test()
        {
            var ilog = new Mock<ILog>();
            var lookupRuleEngine = new LookupRulesEvaluator(ilog.Object);

            var data = new List<KeyValuePair<string, string>>
            {
                 new KeyValuePair<string, string>("expectedStartDate","08012016"),
                 new KeyValuePair<string, string>("reason","fun"),
                new KeyValuePair<string, string>("clientId", "65273"),
                new KeyValuePair<string, string>("employeeNumber", ""),
                new KeyValuePair<string, string>("WorkPhone", "232-232-2345")
            };
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));

            var sortedData = lookupRuleEngine.SortLookupOrder(data, employeeMapping.Mapping, new FieldDefinitionComparer());

            Assert.AreEqual(5, sortedData.Count());
          
            Debug.WriteLine(sortedData.ElementAt(0).Destination);
            Debug.WriteLine(sortedData.ElementAt(1).Destination);
            Debug.WriteLine(sortedData.ElementAt(2).Destination);
            Debug.WriteLine(sortedData.ElementAt(3).Destination);
            Debug.WriteLine(sortedData.ElementAt(4).Destination);
        }

        [TestMethod]
        public void Lookup_Exception_Test()
        {
            var ilog = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult { }

                );

            var lookupRulesEvaluator = new LookupRulesEvaluator(ilog.Object);
            var lookupResolver = new LookupResolver(ilog.Object, httpInvoker.Object,
                new RouteParameterFormatter());
            var transformer = new LookupRouteFieldTransformer(ilog.Object, lookupResolver, lookupRulesEvaluator);

            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var transformedData = transformer.TransformRecordFields(employeeMapping.Mapping,
                Guid.NewGuid().ToString(),
                new Dictionary<string, string>(),
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("clientId","Ad?D"),
                    new KeyValuePair<string, string>("employeeNumber","(47&)"),
                    new KeyValuePair<string, string>("WorkPhone","(232-232-2345)")
                }).ToList();
        }

    }
}
