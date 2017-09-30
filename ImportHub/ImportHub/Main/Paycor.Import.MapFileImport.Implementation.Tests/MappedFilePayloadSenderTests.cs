using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.Http;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Sender;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MappedFilePayloadSenderTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public async Task SendTest_Failure_For_Null_Context()
        {
            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
            )).Returns(() => new HttpExporterResult

            {
                Response = new HttpResponseMessage
                {
                    Content = new StringContent(File.ReadAllText("Json\\GlobalAndClientAliasResponse.json"),
                        Encoding.UTF8, "application/json")
                }

            });

            var apiExecutor = new Mock<IApiExecutor>();
            apiExecutor.Setup(
          t =>
              t.ExecuteApiAsync(It.IsAny<Guid>(), It.IsAny<PayloadData>(),
                  It.IsAny<IDictionary<string, string>>())).Returns(
          () =>
          {
              var result = new ApiResult();
              return Task.FromResult(result);
          });
            var sender = new PayloadSender(logger.Object, apiExecutor.Object);

            var senderResponse = await sender.SendAsync(null, new List<PayloadData>());
            
            Assert.AreEqual(Status.Failure, senderResponse.Status);
        }

        [TestMethod]
        public async Task SendTest_Failure_For_Null_PayLoadDatas()
        {
            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
            )).Returns(() => new HttpExporterResult

            {
                Response = new HttpResponseMessage
                {
                    Content = new StringContent(File.ReadAllText("Json\\GlobalAndClientAliasResponse.json"),
                        Encoding.UTF8, "application/json")
                }

            });

            var apiExecutor = new Mock<IApiExecutor>();
            var sender = new PayloadSender(logger.Object, apiExecutor.Object);
            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = true,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = "1",
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                }
            };

            apiExecutor.Setup(
                t =>
                    t.ExecuteApiAsync(It.IsAny<Guid>(), It.IsAny<PayloadData>(),
                        It.IsAny<IDictionary<string, string>>())).Returns(
                () =>
                {
                    var result = new ApiResult();
                    return Task.FromResult(result);
                });

            
            var senderResponse = await sender.SendAsync(context, null);
            Assert.AreEqual(Status.Failure, senderResponse.Status);
        }

        [TestMethod]
        public async Task Send_For_Batch_Success()
        {
            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpointAsync(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() =>
            {
                var result = new HttpExporterResult();
                result.Response = new HttpResponseMessage
                {
                    Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                };
                return Task.FromResult(result);
            });

            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            var sender = new BatchPayloadSender(logger.Object, httpInvoker.Object, generateFailedRecord.Object);

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = true,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = Guid.NewGuid().ToString(),
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                }
            };

            var apiRecord = new ApiRecord
            {
                Record = new Dictionary<string, string> {{"name", "john"}},
                ApiPayloadArrays = new List<ApiPayloadArray>
                {
                    new ApiPayloadArray()
                    {
                        ArrayData = new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string> {{"country", "usa"}}
                        },
                        ArrayName = "addresses"
                    }
                },
                ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
            };

            var payloadDataItems = new List<PayloadData>
            {
                new PayloadData
                {
                    ApiRecord = apiRecord
                }
            };

        
            var senderResponse = await sender.SendAsync(context, payloadDataItems);

            Assert.AreEqual(Status.Success, senderResponse.Status);
        }


        [TestMethod]
        public async Task Send_For_Batch__Result_Failure()
        {
            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpointAsync(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() =>
            {
                var result = new HttpExporterResult();
                result.Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                        Encoding.UTF8, "application/json")
                };
                return Task.FromResult(result);
            });
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            var sender = new BatchPayloadSender(logger.Object, httpInvoker.Object, generateFailedRecord.Object);

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = true,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = Guid.NewGuid().ToString(),
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                }
            };

            var apiRecord = new ApiRecord
            {
                Record = new Dictionary<string, string> { { "name", "john" } },
                ApiPayloadArrays = new List<ApiPayloadArray>
                {
                    new ApiPayloadArray()
                    {
                        ArrayData = new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string> {{"country", "usa"}}
                        },
                        ArrayName = "addresses"
                    }
                },
                ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
            };

            var payloadDataItems = new List<PayloadData>
            {
                new PayloadData
                {
                    ApiRecord = apiRecord
                }
            };
            var senderResponse = await sender.SendAsync(context, payloadDataItems);

            Assert.AreEqual(Status.Failure, senderResponse.Status);
        }
        [TestMethod]
        public async Task Send_For_Non_Batch_Success()
        {
            var logger = new Mock<ILog>();
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



            var apiExecutor = new Mock<IApiExecutor>();
            apiExecutor.Setup(
             t =>
                 t.ExecuteApiAsync(It.IsAny<Guid>(), It.IsAny<PayloadData>(),
                     It.IsAny<IDictionary<string, string>>())).Returns(
             () =>
             {
                 var result = new ApiResult();
                 return Task.FromResult(result);
             });
            var sender = new PayloadSender(logger.Object, apiExecutor.Object);

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = false,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = Guid.NewGuid().ToString(),
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                },
                ImportHeaderInfo = new Dictionary<string, string> { ["Employee Api"]="1"}
            };

            var apiRecord = new ApiRecord
            {
                ImportType = "Employee Api",
                Record = new Dictionary<string, string> { { "name", "john" } },
                ApiPayloadArrays = new List<ApiPayloadArray>
                {
                    new ApiPayloadArray()
                    {
                        ArrayData = new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string> {{"country", "usa"}}
                        },
                        ArrayName = "addresses"
                    }
                },
                ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
            };

            var payloadDataItems = new List<PayloadData>
            {
                new PayloadData
                {
                    ApiRecord = apiRecord
                }
            };
            var senderResponse = await sender.SendAsync(context, payloadDataItems);

            Assert.AreEqual(Status.Success, senderResponse.Status);
        }


        [TestMethod]
        public async Task Send_For_Non_Batch_API_Call_Returns_Failure()
        {
            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpointAsync(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() =>
            {
                var result = new HttpExporterResult

            {
                Response = new HttpResponseMessage()
            };
                result.Response.Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                    Encoding.UTF8, "application/json");
                result.Response.StatusCode = HttpStatusCode.Forbidden;
                return Task.FromResult(result);
            });
            var apiExecutor = new Mock<IApiExecutor>();
            var sender = new PayloadSender(logger.Object, apiExecutor.Object);

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = false,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = Guid.NewGuid().ToString(),
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                },
                ImportHeaderInfo = new Dictionary<string, string> { ["Employee Api"]="1"}
            };

            var apiRecord = new ApiRecord
            {
                ImportType = "Employee Api",
                Record = new Dictionary<string, string> { { "name", "john" } },
                ApiPayloadArrays = new List<ApiPayloadArray>
                {
                    new ApiPayloadArray()
                    {
                        ArrayData = new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string> {{"country", "usa"}}
                        },
                        ArrayName = "addresses"
                    }
                },
                ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
            };

            var payloadDataItems = new List<PayloadData>
            {
                new PayloadData
                {
                    ApiRecord = apiRecord
                }
            };

            
            apiExecutor.Setup(
             t =>
                 t.ExecuteApiAsync(It.IsAny<Guid>(), It.IsAny<PayloadData>(),
                     It.IsAny<IDictionary<string, string>>())).Returns(
             () =>
             {
                 var result = new ApiResult
                 {
                     ErrorResultDataItem = new ErrorResultData()
                     {
                         HttpExporterResult = new HttpExporterResult()
                         {
                             Response = new HttpResponseMessage()
                             {
                                 StatusCode = HttpStatusCode.Forbidden
                             }
                         }
                     }
                 };

                 return Task.FromResult(result);
             });
            var senderResponse = await sender.SendAsync(context, payloadDataItems);


            Assert.AreEqual(HttpStatusCode.Forbidden,senderResponse.ErrorResultDataItems.FirstOrDefault()
                .HttpExporterResult.Response.StatusCode);
            Assert.AreEqual(Status.Success, senderResponse.Status);
        }

        [TestMethod]
        public async Task Send_For_Non_Batch_API_Call_Returns_Invalid_HttpResponse()
        {
            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult

                {
                   Response = null,
                   
                });
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            generateFailedRecord.Setup(t=>t.GetFailedRecord
            (It.IsAny<ApiRecord>(),It.IsAny<ErrorResponse>(),It.IsAny<HttpExporterResult>())).Returns(new FailedRecord());
            var calculator = new Mock<ICalculate>();
            var apiExecutor = new Mock<IApiExecutor>();
            var sender = new PayloadSender(logger.Object, apiExecutor.Object);

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = false,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = Guid.NewGuid().ToString(),
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                },
                ImportHeaderInfo = new Dictionary<string, string> { ["Test"]="1"}
            };

            var apiRecord = new ApiRecord
            {
            
                Record = new Dictionary<string, string> { { "name", "john" } },
                ApiPayloadArrays = new List<ApiPayloadArray>
                {
                    new ApiPayloadArray()
                    {
                        ArrayData = new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string> {{"country", "usa"}}
                        },
                        ArrayName = "addresses"
                    }
                },
                ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
            };

            var payloadDataItems = new List<PayloadData>
            {
                new PayloadData
                {
                    ApiRecord = apiRecord
                },
                null
            };
            apiExecutor.Setup(
             t =>
                 t.ExecuteApiAsync(It.IsAny<Guid>(), It.IsAny<PayloadData>(),
                     It.IsAny<IDictionary<string, string>>())).Returns(
             () =>
             {
                 var result = new ApiResult();
                 return Task.FromResult(result);
             });
            var senderResponse = await sender.SendAsync(context, payloadDataItems);
            
            Assert.AreEqual(Status.Success, senderResponse.Status);
        }


        [TestMethod]
        public async Task Send_For_Non_Batch_API_Call_Handles_Exception_Properly()
        {
            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
            )).Throws(new Exception());
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            generateFailedRecord.Setup(t => t.GetFailedRecord
            (It.IsAny<ApiRecord>(), It.IsAny<ErrorResponse>(), It.IsAny<HttpExporterResult>())).Returns(new FailedRecord());
            var apiExecutor = new Mock<IApiExecutor>();
            var sender = new PayloadSender(logger.Object, apiExecutor.Object);

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = false,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = Guid.NewGuid().ToString(),
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                },
                ImportHeaderInfo = new Dictionary<string, string> { ["Test"]="1"}
            };

            var apiRecord = new ApiRecord
            {

                Record = new Dictionary<string, string> { { "name", "john" } },
                ApiPayloadArrays = new List<ApiPayloadArray>
                {
                    new ApiPayloadArray()
                    {
                        ArrayData = new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string> {{"country", "usa"}}
                        },
                        ArrayName = "addresses"
                    }
                },
                ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
            };

            var payloadDataItems = new List<PayloadData>
            {
                new PayloadData
                {
                    ApiRecord = apiRecord
                },
                null
            };

            apiExecutor.Setup(
           t =>
               t.ExecuteApiAsync(It.IsAny<Guid>(), It.IsAny<PayloadData>(),
                   It.IsAny<IDictionary<string, string>>())).Returns(
           () =>
           {
               var result = new ApiResult();
               return Task.FromResult(result);
           });

            var senderResponse = await sender.SendAsync(context, payloadDataItems);

            Assert.AreEqual(Status.Success, senderResponse.Status);
        }

        [TestMethod]
        public void Send_Batch_Chunk_Record()
        {
            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = true,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = Guid.NewGuid().ToString(),
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}
                },
                ImportHeaderInfo = new Dictionary<string, string> { ["Test Import"]="3"}
            };
            var apirecords = new List<ApiRecord>
            {
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10052" },
                        {"Name","Ashif" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 1
                },
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10043" },
                        {"Name","Anwar" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 2
                },
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10044" },
                        {"Name","Mark Yang" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 3
                }
            };
            var payloadDataItems = new List<PayloadData>
            {
                new PayloadData
                {                    
                  ApiRecords = apirecords,                                      
                }
            };

            var errorResponseBatch = new List<ErrorResponseDataBatch>
            {
               new ErrorResponseDataBatch
               {
                   Status = "Failure",
                   BatchLine = 1,
                   ErrorResponseBatchRowEntry = new ErrorResponseBatchRowEntry
                   {
                       Source = null,
                       Detail = "Char cannot be more than 12",
                       Title = "Validation Errors"
                   }
                 
               },
               new ErrorResponseDataBatch
               {
                   Status = "Forbidden",
                   BatchLine = 2,
                   ErrorResponseBatchRowEntry = new ErrorResponseBatchRowEntry
                   {
                       Source = null,
                       Detail = "Not allowed to import this record",
                       Title = "Un-authoirized"
                   }
               },

            };
            var batchImportResponse = new BatchImportResponse
            {
                CorrelationId = Guid.NewGuid(),
                ErrorResponseDataBatch = errorResponseBatch
            };
            
            var jsonString = JsonConvert.SerializeObject(batchImportResponse);
            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpoint(It.IsAny<Guid>(), It.IsAny<string>()
                ,It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
                )).Returns(() => new HttpExporterResult
                {
                    Response = new HttpResponseMessage
                    {
                        Content = new StringContent(jsonString)
                    }
                });

            var apiExecutor = new Mock<IApiExecutor>();
            apiExecutor.Setup(
              t =>
                  t.ExecuteApiAsync(It.IsAny<Guid>(), It.IsAny<PayloadData>(),
                      It.IsAny<IDictionary<string, string>>())).Returns(
              () =>
              {
                  var result = new ApiResult();
                  return Task.FromResult(result);
              });
            var sender = new PayloadSender(logger.Object, apiExecutor.Object);

            var senderResponse = sender.SendAsync(context, payloadDataItems);
            foreach (var errorObject in senderResponse.Result.ErrorResultDataItems)
            {
                Assert.AreEqual(errorObject.FailedRecord.CustomData.Count, 2);
                Assert.AreEqual(errorObject.ImportType, "Test Import");
            }
        }
    }
}
