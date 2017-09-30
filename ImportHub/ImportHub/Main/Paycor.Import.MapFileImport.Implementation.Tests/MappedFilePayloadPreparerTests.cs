using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Preparer;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappedFilePayloadPreparerTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void PrepareTest_For_Failure()
        {
            var logger = new Mock<ILog>();
            var transfomer = new Mock<IFlatToApiRecordsTransformer>();

            var routeParamFormatter = new Mock<IRouteParameterFormatter>();
            var apiRecordJsonGenerator = new Mock<IApiRecordJsonGenerator>();
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            var calculator = new Mock<ICalculate>();
            var preparer = new MappedFilePayloadPreparer(logger.Object, transfomer.Object,
                new PreparePayloadFactory(routeParamFormatter.Object, logger.Object, apiRecordJsonGenerator.Object,
                    generateFailedRecord.Object, calculator.Object));

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = false,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = "1",
                TransactionId = "1",
            };

            var chunkDataSource = new Mock<IChunkDataSource>();
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var preparedData = preparer.Prepare(context, employeeMapping, chunkDataSource.Object);

            Assert.AreEqual(Status.Failure,preparedData.Status);

        }

        [TestMethod]
        public void PrepareTest_For_Batch_Success()
        {
            var logger = new Mock<ILog>();
            var apiRecords = new List<ApiRecord>()
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

            var flatToApiRecordTransformer = new Mock<IFlatToApiRecordsTransformer>();
            flatToApiRecordTransformer.Setup(t => t.TranslateFlatRecordsToApiRecords(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<GeneratedMapping>(),
                It.IsAny<ImportContext>()
            )).Returns(apiRecords);

            var routeParamFormatter = new Mock<IRouteParameterFormatter>();
            routeParamFormatter.Setup(t => t.FormatAllEndPointsWithParamValue(It.IsAny<IDictionary<HtmlVerb, string>>()
                    , It.IsAny<IDictionary<string, string>>()
                ))
                .Returns(new Dictionary<HtmlVerb, string>
                {
                    { HtmlVerb.Post,
                    "http://localhost/employeeservice/v2/employees"
                    }
                });
            var apiRecordJsonGenerator = new Mock<IApiRecordJsonGenerator>();
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            var calculator = new Mock<ICalculate>();
            var preparer = new MappedFilePayloadPreparer(logger.Object, flatToApiRecordTransformer.Object,
                new PreparePayloadFactory(routeParamFormatter.Object, logger.Object, apiRecordJsonGenerator.Object,
                    generateFailedRecord.Object, calculator.Object));

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

            var chunkDataSource = new Mock<IChunkDataSource>();
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var preparedData = preparer.Prepare(context, employeeMapping, chunkDataSource.Object);

            Assert.AreEqual(Status.Success, preparedData.Status);
            Assert.AreEqual(1, preparedData.PayloadDataItems.Count());

        }
        [TestMethod]
        public void PrepareTest_For_NonBatch_Post_Success()
        {
            var logger = new Mock<ILog>();
            var apiRecords = new List<ApiRecord>()
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
                    Record = null,
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

            var flatToApiRecordTransformer = new Mock<IFlatToApiRecordsTransformer>();
            flatToApiRecordTransformer.Setup(t => t.TranslateFlatRecordsToApiRecords(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<GeneratedMapping>(),
                It.IsAny<ImportContext>()
            )).Returns(apiRecords);
              
            var routeParamFormatter = new Mock<IRouteParameterFormatter>();
            routeParamFormatter.Setup(t => t.FormatAllEndPointsWithParamValue(It.IsAny<IDictionary<HtmlVerb, string>>()
                    , It.IsAny<IDictionary<string, string>>()
                ))
                .Returns(new Dictionary<HtmlVerb, string>
                {
                    { HtmlVerb.Post,
                    "http://localhost/employeeservice/v2/employees"
                    }
                });
            var apiRecordJsonGenerator = new Mock<IApiRecordJsonGenerator>();
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            var calculator = new Mock<ICalculate>();
            var preparer = new MappedFilePayloadPreparer(logger.Object, flatToApiRecordTransformer.Object,
                new PreparePayloadFactory(routeParamFormatter.Object, logger.Object, apiRecordJsonGenerator.Object,
                    generateFailedRecord.Object, calculator.Object));

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = false,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = "1",
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                }
            };

            var chunkDataSource = new Mock<IChunkDataSource>();
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var preparedData = preparer.Prepare(context, employeeMapping, chunkDataSource.Object);

            Assert.AreEqual(Status.Success, preparedData.Status);
            Assert.AreEqual(2,preparedData.PayloadDataItems.Count());

        }

        [TestMethod]
        public void PrepareTest_For_NonBatch_Patch_Success()
        {
            var logger = new Mock<ILog>();
            var apiRecords = new List<ApiRecord>()
            {
                new ApiRecord
                {
                   
                    Record = new Dictionary<string, string> { {"name","john" } , {"ih:action","c"} },
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
                    IsPayloadMissing = true,
                    Record = new Dictionary<string, string> { {"name","john" } , {"ih:action","c"} },
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
                    Record = null,
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "country","france"} }
                            },
                            ArrayName = "addresses"
                        }
                    },
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
                },
                     new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"name","john" } , {"ih:action","p"}
                    },
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

            var flatToApiRecordTransformer = new Mock<IFlatToApiRecordsTransformer>();
            flatToApiRecordTransformer.Setup(t => t.TranslateFlatRecordsToApiRecords(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<GeneratedMapping>(),
                It.IsAny<ImportContext>()
            )).Returns(apiRecords);

            var routeParamFormatter = new Mock<IRouteParameterFormatter>();
            routeParamFormatter.Setup(t => t.FormatAllEndPointsWithParamValue(It.IsAny<IDictionary<HtmlVerb, string>>()
                    , It.IsAny<IDictionary<string, string>>()
                ))
                .Returns(new Dictionary<HtmlVerb, string>
                {
                    { HtmlVerb.Patch,
                    "http://localhost/employeeservice/v2/employees"
                    }
                });
            var apiRecordJsonGenerator = new Mock<IApiRecordJsonGenerator>();
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            var calculator = new Mock<ICalculate>();
            var preparer = new MappedFilePayloadPreparer(logger.Object, flatToApiRecordTransformer.Object,
                new PreparePayloadFactory(routeParamFormatter.Object, logger.Object, apiRecordJsonGenerator.Object,
                    generateFailedRecord.Object, calculator.Object));

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = false,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = "1",
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Patch, "http://localhost/employeeservice/v2/employees"}

                }
            };

            var chunkDataSource = new Mock<IChunkDataSource>();
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var preparedData = preparer.Prepare(context, employeeMapping, chunkDataSource.Object);

            Assert.AreEqual(Status.Success, preparedData.Status);
            Assert.AreEqual(1, preparedData.PayloadDataItems.Count());

        }

        [TestMethod]
        public void PrepareTest_For_NonBatch_Patch_InvalidEndPoint()
        {
            var logger = new Mock<ILog>();
            var apiRecords = new List<ApiRecord>()
            {
                new ApiRecord
                {

                    Record = new Dictionary<string, string> { {"name","john" } , {"ih:action","c"} },
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
                    IsPayloadMissing = true,
                    Record = new Dictionary<string, string> { {"name","john" } , {"ih:action","c"} },
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
                    Record = null,
                    ApiPayloadArrays =  new List<ApiPayloadArray>
                    {
                        new ApiPayloadArray()
                        {
                            ArrayData = new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "country","france"} }
                            },
                            ArrayName = "addresses"
                        }
                    },
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>()
                },
                     new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"name","john" } , {"ih:action","p"}
                    },
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

            var flatToApiRecordTransformer = new Mock<IFlatToApiRecordsTransformer>();
            flatToApiRecordTransformer.Setup(t => t.TranslateFlatRecordsToApiRecords(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<GeneratedMapping>(),
                It.IsAny<ImportContext>()
            )).Returns(apiRecords);

            var routeParamFormatter = new Mock<IRouteParameterFormatter>();
            routeParamFormatter.Setup(t => t.FormatAllEndPointsWithParamValue(It.IsAny<IDictionary<HtmlVerb, string>>()
                    , It.IsAny<IDictionary<string, string>>()
                ))
                .Returns(new Dictionary<HtmlVerb, string>
                {
                    { HtmlVerb.Patch,
                    "http://localhost/employeeservice/v2/employees"
                    }
                });
            routeParamFormatter.Setup(t => t.IsEndPointInvalidForOperation(It.IsAny<string>()
               ))
               .Returns(true);
            var apiRecordJsonGenerator = new Mock<IApiRecordJsonGenerator>();
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            var calculator = new Mock<ICalculate>();
            var preparer = new MappedFilePayloadPreparer(logger.Object, flatToApiRecordTransformer.Object,
                new PreparePayloadFactory(routeParamFormatter.Object, logger.Object, apiRecordJsonGenerator.Object,
                    generateFailedRecord.Object, calculator.Object));

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
                    {HtmlVerb.Patch, "http://localhost/employeeservice/v2/employees"}

                }
            };

            var chunkDataSource = new Mock<IChunkDataSource>();
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var preparedData = preparer.Prepare(context, employeeMapping, chunkDataSource.Object);

            Assert.AreEqual(Status.Success, preparedData.Status);
            Assert.AreEqual(0, preparedData.PayloadDataItems.Count());

        }

        [TestMethod]
        public void PrepareTest_For_NonBatch_Upsert()
        {
            var logger = new Mock<ILog>();
            var apiRecords = new List<ApiRecord>
            {
                new ApiRecord
                {

                    Record = new Dictionary<string, string> { {"name","john" } , {"ih:action","u"} },
                    ApiPayloadArrays =  new List<ApiPayloadArray>(),
                },  
                new ApiRecord
                {
                   
                    Record = new Dictionary<string, string> { {"name","john" } , {"ih:action","u"} },
                    ApiPayloadArrays =  new List<ApiPayloadArray>(),
                }
            };
            var records = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { {"name","john" } , {"ih:action","u"} },
                new Dictionary<string, string> { {"name","john" } , {"ih:action","u"} }
            };

            var flatToApiRecordTransformer = new Mock<IFlatToApiRecordsTransformer>();
            flatToApiRecordTransformer.Setup(t => t.TranslateFlatRecordsToApiRecords(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<GeneratedMapping>(),
                It.IsAny<ImportContext>()
            )).Returns(apiRecords);

            var routeParamFormatter = new Mock<IRouteParameterFormatter>();
            routeParamFormatter.Setup(t => t.FormatAllEndPointsWithParamValue(It.IsAny<IDictionary<HtmlVerb, string>>()
                    , It.IsAny<IDictionary<string, string>>()
                ))
                .Returns(new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Patch, "http://localhost/employeeservice/v2/employees/1234-1234-12344-4566"},
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"},
                    {HtmlVerb.Put, "http://localhost/employeeservice/v2/employees/1234-1234-12344-4566"}
                });

            var context = new ImportContext
            {
                HasHeader = true,
                CallApiInBatch = false,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = "1",
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"},
                    {HtmlVerb.Put, "http://localhost/employeeservice/v2/employees/1234-1234-12344-4566"},
                    {HtmlVerb.Patch, "http://localhost/employeeservice/v2/employees/1234-1234-12344-4566"}
                }
            };
            var employeeMapping = JsonConvert.DeserializeObject<GeneratedMapping>(File.ReadAllText("Json\\EmployeeApiMapping.json"));
            var apiRecordJsonGenerator = new Mock<IApiRecordJsonGenerator>();
            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            var calculator = new Mock<ICalculate>();
            var chunkDataSource = new Mock<IChunkDataSource>();
            chunkDataSource.Setup(t => t.Records).Returns(records);
            var preparer = new MappedFilePayloadPreparer(logger.Object, flatToApiRecordTransformer.Object,
                new PreparePayloadFactory(routeParamFormatter.Object, logger.Object, apiRecordJsonGenerator.Object,
                    generateFailedRecord.Object, calculator.Object));
            var preparerResponse = preparer.Prepare(context, employeeMapping, chunkDataSource.Object);
            Assert.AreEqual(Status.Success, preparerResponse.Status);
        }
    }
}
