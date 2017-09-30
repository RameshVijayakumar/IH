using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import;
using Paycor.Import.Employee.Status;
using Paycor.Import.Http;
using Paycor.Import.ImportHistory;
using Paycor.Import.Messaging;
using Paycor.Import.Status;

namespace EmployeeImportWorker.Test
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EmployeeImportExporterTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestClass]
        public class Initialize_Tests
        {
            //This is a test comment.
            [TestMethod]
            public void EmployeeImportExporter_Initialize_Success()
            {
                const string expectedBaseAddress = "http://localhost";
                const string expectedCompanyDomainBaseAddress = "http://localhost/Company";
                const string expectedApiEndpoint = "PerformService/Api/EmployeeImport";
                const string expectedCompanyDomainApiEndpoint = "Paycor.CompanyDomain.Service/company";

                var log = new Mock<ILog>();
                var storageProvider = new Mock<IStatusStorageProvider>();
                var employeeImportExporter = new EmployeeImportExporter(log.Object, storageProvider.Object,
                    new Mock<IImportHistoryService>().Object, ImportConstants.DefaultHttpTimeout,
                    new Mock<INotificationMessageClient>().Object);
                var configSettings = new NameValueCollection
                {
                    {EmployeeImportExporter.BaseAddressKey, expectedBaseAddress},
                    {EmployeeImportExporter.EmployeeImportApiEndpointKey, expectedApiEndpoint},
                    {EmployeeImportExporter.CompanyDomainBaseAddressKey, expectedCompanyDomainBaseAddress},
                    {EmployeeImportExporter.CompanyDomainApiEndpointKey, expectedCompanyDomainApiEndpoint},
                };

                var actual = employeeImportExporter.Initialize(configSettings);
                var privateObject = new PrivateObject(employeeImportExporter);
                var baseAddress = privateObject.GetField("_baseAddress") as String;
                var apiEndpoint = privateObject.GetField("_apiEndpoint") as String;
                var companyDomainBaseAddress = privateObject.GetField("_companyDomainBaseAddress") as String;
                var companyDomainApiEndpoint = privateObject.GetField("_companyDomainApiEndpoint") as String;

                Assert.IsTrue(actual);
                Assert.IsNotNull(baseAddress);
                Assert.IsNotNull(apiEndpoint);
                Assert.AreEqual(expectedBaseAddress, baseAddress);
                Assert.AreEqual(expectedApiEndpoint, apiEndpoint);
                Assert.AreEqual(expectedCompanyDomainBaseAddress, companyDomainBaseAddress);
                Assert.AreEqual(expectedCompanyDomainApiEndpoint, companyDomainApiEndpoint);
            }

            [TestMethod]
            public void EmployeeImportExporter_Initialize_BaseAddress_Empty()
            {
                const string expectedBaseAddress = "";
                const string expectedApiEndpoint = "PerformService/Api/EmployeeImport";

                var log = new Mock<ILog>();
                var storageProvider = new Mock<IStatusStorageProvider>();
                var employeeImportExporter = new EmployeeImportExporter(log.Object, storageProvider.Object,
                    new Mock<IImportHistoryService>().Object, ImportConstants.DefaultHttpTimeout,
                    new Mock<INotificationMessageClient>().Object);
                var configSettings = new NameValueCollection
                {
                    {EmployeeImportExporter.BaseAddressKey, expectedBaseAddress},
                    {EmployeeImportExporter.EmployeeImportApiEndpointKey, expectedApiEndpoint},
                };

                var actual = employeeImportExporter.Initialize(configSettings);

                Assert.IsFalse(actual);
            }

            [TestMethod]
            public void EmployeeImportExporter_Initialize_ApiEndpoint_Empty()
            {
                const string expectedBaseAddress = "http://localhost";
                const string expectedApiEndpoint = "";

                var log = new Mock<ILog>();
                var storageProvider = new Mock<IStatusStorageProvider>();
                var employeeImportExporter = new EmployeeImportExporter(log.Object, storageProvider.Object,
                    new Mock<IImportHistoryService>().Object, ImportConstants.DefaultHttpTimeout,
                    new Mock<INotificationMessageClient>().Object);
                var configSettings = new NameValueCollection
                {
                    {EmployeeImportExporter.BaseAddressKey, expectedBaseAddress},
                    {EmployeeImportExporter.EmployeeImportApiEndpointKey, expectedApiEndpoint},
                };

                var actual = employeeImportExporter.Initialize(configSettings);

                Assert.IsFalse(actual);
            }
        }

        [TestClass]
        public class OnExport_Tests
        {
            private const string Container = "employeeimportstatuscontainer";
            private const string FileName = "66670txt";
            private const string ExpectedApiEndpoint = "my/url/endpoint";
            private const string BaseAddress = "http://localhost";
            private const string ApiEndpoint = "my/url/endpoint";
            private const string ClientId = "66670";

            private readonly string _masterSessionId = new Guid().ToString();

            private static readonly string TransactionId = Guid.NewGuid().ToString();

            private static Mock<IStatusStorageProvider> _storageProvider;
            private static StatusManager<EmployeeImportStatusMessage> _manager;
            private static ImportStatusReceiver<EmployeeImportStatusMessage> _receiver;
            private static ImportStatusRetriever<EmployeeImportStatusMessage> _retriever;
            private static Mock<IImportHistoryService> _importHistoryService;
            private static readonly List<StatusMessage> StatusMessageStore = new List<StatusMessage>();
            private static Mock<ILog> _log;

            private class EEImportPayload
            {
                public string ClientId { get; set; }
                public EmployeeImportStatusEnum Status { get; set; }
                public IEnumerable<IDictionary<string, string>> ImportData { get; set; }
            }

            private class TestEmployeeImportExporter : EmployeeImportExporter
            {
                public TestEmployeeImportExporter(ILog log, IStatusStorageProvider storageProvider)
                    : base(
                        log, storageProvider, new Mock<IImportHistoryService>().Object,
                        ImportConstants.DefaultHttpTimeout, new Mock<INotificationMessageClient>().Object)
                {
                }

                public int TimesCalled { get; private set; }
                public Task<HttpExporterResult> CurrentExporterResult { get; private set; }
                public string ApiEndpoint { get; private set; }
                public string PostPayload { get; private set; }

                public void TestExport(RestApiPayload payload)
                {
                    CurrentExporterResult = OnExportAsync(payload);
                }

                protected override Client CallCompanyDomain(string clientId)
                {
                    return new Client();
                }

                protected override async Task<HttpExporterResult> PostToApiAsync(string jsonData, string apiEndpoint)
                {
                    TimesCalled += 1;
                    PostPayload = jsonData;
                    ApiEndpoint = apiEndpoint;

                    var payload = JsonConvert.DeserializeObject<EEImportPayload>(jsonData);
                    var records = payload.ImportData;
                    var clientId = payload.ClientId;
                    IEnumerable<IDictionary<string, string>> validationReport = null;
                    IEnumerable<IDictionary<string, int>> validationTotals = null;

                    if (records != null)
                    {
                        validationReport = records.Select(record => new Dictionary<string, string>
                        {
                            {"EmployeeNumber", record["EmployeeNumber"]},
                            {"LastName", record["LastName"]},
                            {"Description", string.Empty}
                        }).Cast<IDictionary<string, string>>().ToList();

                        validationTotals = new List<IDictionary<string, int>>
                        {
                            new Dictionary<string, int>
                            {
                                {"TotalRecordsImported", records.Count()},
                                {"TotalProcessed", records.Count()}
                            }
                        };
                    }

                    var item = new
                    {
                        ValidationTotals = validationTotals,
                        ValidationReport = validationReport,
                        EmployeeUpdateCount = 0,
                        HasDuplicateEmployees = false,
                    };

                    var responseContent = JsonConvert.SerializeObject(item);
                    var CurrentExporterResult = new HttpExporterResult
                    {
                        ClientId = clientId,
                        Exception = null,
                        Response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.Accepted,
                            Content = new StringContent(responseContent)
                        },
                    };

                    return CurrentExporterResult;
                }
            }

            private class TestEmployeeImportExporterInvalidClient : TestEmployeeImportExporter
            {

                public TestEmployeeImportExporterInvalidClient(ILog log, IStatusStorageProvider storageProvider)
                    : base(log, storageProvider)
                {
                }

                protected override Client CallCompanyDomain(string clientId)
                {
                    return null;
                }
            }

            [TestMethod]
            public void EmployeeImportExporter_OnExport_InValidClient()
            {
                var actualMessage = string.Empty;
                var expectedMessage = EmployeeImportResource.EEImportInvalidClientId;
                var restApiPayload = new EeImportRestApiPayload()
                {
                    ApiEndpoint = ExpectedApiEndpoint,
                    Name = FileName,
                    TransactionId = TransactionId,
                    MasterSessionId = _masterSessionId,
                    Records = new List<IDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Mary"},
                            {"LastName", "Jones"},
                            {"EmployeeNumber", "450"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "John"},
                            {"LastName", "Glenn"},
                            {"EmployeeNumber", "866"}
                        }
                    }
                };

                var configSettings = new NameValueCollection
                {
                    {EmployeeImportExporter.BaseAddressKey, BaseAddress},
                    {EmployeeImportExporter.EmployeeImportApiEndpointKey, ApiEndpoint},
                };

                var log = new Mock<ILog>();
                var testExporter = new TestEmployeeImportExporterInvalidClient(log.Object, _storageProvider.Object);

                testExporter.TestExport(restApiPayload);
                log.Verify(
                    m => m.Error("An exception occurred during the export process.", It.IsAny<ImportException>()),
                    Times.Once());
            }

            private static void CreateOrUpdate(StatusMessage message)
            {
                if (StatusMessageStore.Count > 0)
                {
                    var index = StatusMessageStore.FindIndex(x => x.Reporter == Container && x.Key == TransactionId);

                    if (index >= 0)
                    {
                        StatusMessageStore[index] = message;
                    }
                    else
                    {
                        StatusMessageStore.Add(message);
                    }
                }
                else
                {
                    StatusMessageStore.Add(message);
                }
            }

            [TestInitialize]
            public void Setup()
            {
                _storageProvider = new Mock<IStatusStorageProvider>();
                _storageProvider.Setup(e => e.StoreStatus(It.IsAny<StatusMessage>()))
                    .Callback<StatusMessage>(CreateOrUpdate);
                _storageProvider.Setup(e => e.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(
                        () => StatusMessageStore.SingleOrDefault(x => x.Reporter == Container && x.Key == TransactionId));

                _receiver = new ImportStatusReceiver<EmployeeImportStatusMessage>(Container);
                _manager = new StatusManager<EmployeeImportStatusMessage>(_receiver, _storageProvider.Object);
                _retriever = new ImportStatusRetriever<EmployeeImportStatusMessage>(_storageProvider.Object, Container);
                _importHistoryService = new Mock<IImportHistoryService>();
                var message = new EmployeeImportStatusMessage
                {
                    ClientId = "123",
                    FileName = FileName,
                    Id = TransactionId
                };
                CreateOrUpdate(new StatusMessage
                {
                    Key = TransactionId,
                    Reporter = Container,
                    Status = JsonConvert.SerializeObject(message)
                });
                _log = new Mock<ILog>();
            }

            [TestMethod]
            public void EmployeeImportExporter_OnExport_Success()
            {
                var restApiPayload = new EeImportRestApiPayload()
                {
                    MappingValue = ImportConstants.EeImportMappingEnum.ContinueImport.ToString(),
                    ApiEndpoint = ExpectedApiEndpoint,
                    Name = FileName,
                    TransactionId = TransactionId,
                    MasterSessionId = _masterSessionId,
                    Records = new List<IDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Mary"},
                            {"LastName", "Jones"},
                            {"EmployeeNumber", "450"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "John"},
                            {"LastName", "Glenn"},
                            {"EmployeeNumber", "866"}
                        }
                    }
                };

                var configSettings = new NameValueCollection
                {
                    {EmployeeImportExporter.BaseAddressKey, BaseAddress},
                    {EmployeeImportExporter.EmployeeImportApiEndpointKey, ApiEndpoint},
                };

                var log = new Mock<ILog>();
                var testExporter = new TestEmployeeImportExporter(log.Object, _storageProvider.Object);
                testExporter.Initialize(configSettings);

                testExporter.TestExport(restApiPayload);

                const int expectedCount = 23;
                var actualResult = testExporter.CurrentExporterResult;
                var actualContent = actualResult.Result.Response.Content.ReadAsStringAsync().Result;
                const string expectedContent =
                    "{\"ValidationTotals\":null,\"ValidationReport\":null,\"EmployeeUpdateCount\":0,\"HasDuplicateEmployees\":false}";
                Assert.AreEqual(ClientId, actualResult.Result.ClientId);
                Assert.AreEqual(expectedContent, actualContent);
                Assert.AreEqual(ExpectedApiEndpoint, testExporter.ApiEndpoint,
                    "The API endpoint that was used to create the Rest API exporter was passed to the PostToApi call.");
                Assert.AreEqual(expectedCount, testExporter.TimesCalled);
            }

            [TestMethod]
            public void EmployeeImportExporter_OnExport_MultipleChunks()
            {
                var restApiPayload = new EeImportRestApiPayload()
                {
                    MappingValue = ImportConstants.EeImportMappingEnum.ContinueImport.ToString(),
                    ApiEndpoint = ExpectedApiEndpoint,
                    Name = FileName,
                    TransactionId = TransactionId,
                    MasterSessionId = _masterSessionId,
                    Records = new List<IDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Mary"},
                            {"LastName", "Jones"},
                            {"EmployeeNumber", "100"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Gus"},
                            {"LastName", "Grisom"},
                            {"EmployeeNumber", "200"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Thomas"},
                            {"LastName", "Stafford"},
                            {"EmployeeNumber", "300"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Joe"},
                            {"LastName", "Smith"},
                            {"EmployeeNumber", "500"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Fred"},
                            {"LastName", "Mercury"},
                            {"EmployeeNumber", "500"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Jimmy"},
                            {"LastName", "Johns"},
                            {"EmployeeNumber", "600"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Tom"},
                            {"LastName", "Slick"},
                            {"EmployeeNumber", "700"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Ralph"},
                            {"LastName", "Nader"},
                            {"EmployeeNumber", "800"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "John"},
                            {"LastName", "Glenn"},
                            {"EmployeeNumber", "900"}
                        }
                    }
                };

                var configSettings = new NameValueCollection
                {
                    {EmployeeImportExporter.BaseAddressKey, BaseAddress},
                    {EmployeeImportExporter.EmployeeImportApiEndpointKey, ApiEndpoint},
                    {EmployeeImportExporter.ChunkSizeKey, "4"}
                };

                var log = new Mock<ILog>();
                var testExporter = new TestEmployeeImportExporter(log.Object, _storageProvider.Object);

                testExporter.Initialize(configSettings);
                testExporter.TestExport(restApiPayload);

                const int expectedCount = 25;
                var actualResult = testExporter.CurrentExporterResult;
                var actualContent = actualResult.Result.Response.Content.ReadAsStringAsync().Result;
                const string expectedContent =
                    "{\"ValidationTotals\":null,\"ValidationReport\":null,\"EmployeeUpdateCount\":0,\"HasDuplicateEmployees\":false}";
                Assert.AreEqual(ClientId, actualResult.Result.ClientId);
                Assert.AreEqual(expectedContent, actualContent);
                Assert.AreEqual(ExpectedApiEndpoint, testExporter.ApiEndpoint,
                    "The API endpoint that was used to create the Rest API exporter was passed to the PostToApi call.");
                Assert.AreEqual(expectedCount, testExporter.TimesCalled);
            }

            [TestMethod]
            public void EmployeeImportExporter_OnExport_WrongPayloadType()
            {
                var restApiPayload = new EeImportRestApiPayload()
                {
                    ApiEndpoint = ExpectedApiEndpoint,
                    Name = FileName,
                    TransactionId = TransactionId,
                    Records = new List<IDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "Mary"},
                            {"LastName", "Jones"},
                            {"EmployeeNumber", "450"}
                        },
                        new Dictionary<string, string>
                        {
                            {"ClientId", ClientId},
                            {"FirstName", "John"},
                            {"LastName", "Glenn"},
                            {"EmployeeNumber", "866"}
                        }
                    },
                    MappingValue = ImportConstants.EeImportMappingEnum.ContinueImport.ToString()
                };

                var configSettings = new NameValueCollection
                {
                    {EmployeeImportExporter.BaseAddressKey, BaseAddress},
                    {EmployeeImportExporter.EmployeeImportApiEndpointKey, ApiEndpoint},
                };

                var log = new Mock<ILog>();
                var testExporter = new TestEmployeeImportExporter(log.Object, _storageProvider.Object);

                testExporter.Initialize(configSettings);
                testExporter.TestExport(restApiPayload);
                log.Verify(
                    m => m.Error("An exception occurred during the export process.", It.IsAny<ArgumentNullException>()),
                    Times.Once());
            }

            [TestMethod]
            public void EmployeeImportExporter_OnExport_NoRecords()
            {
                var restApiPayload = new EeImportRestApiPayload()
                {
                    ApiEndpoint = ExpectedApiEndpoint,
                    Name = FileName,
                    TransactionId = TransactionId,
                    MasterSessionId = _masterSessionId,
                    Records = new List<IDictionary<string, string>>()
                };

                var configSettings = new NameValueCollection
                {
                    {EmployeeImportExporter.BaseAddressKey, BaseAddress},
                    {EmployeeImportExporter.EmployeeImportApiEndpointKey, ApiEndpoint},
                };

                var log = new Mock<ILog>();
                var testExporter = new TestEmployeeImportExporter(log.Object, _storageProvider.Object);

                testExporter.Initialize(configSettings);
                testExporter.TestExport(restApiPayload);
                log.Verify(
                    m => m.Error("An exception occurred during the export process.", It.IsAny<ArgumentException>()),
                    Times.Once());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmployeeImportExporter_Enforce_SaveStatusFromApi_Record()
            {
                const EmployeeImportStatusEnum status = EmployeeImportStatusEnum.EmployeeImportUpdateBenefits;
                const int payloadRecordCount = 6;
                var statusLogger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager,
                    _retriever, _importHistoryService.Object);

                var log = new Mock<ILog>();
                var mockClient = new Mock<INotificationMessageClient>();
                var exporter = new PrivateObject(typeof(EmployeeImportExporter), log.Object, _storageProvider.Object,
                    new Mock<IImportHistoryService>().Object, ImportConstants.DefaultHttpTimeout, mockClient.Object);

                var actual =
                    exporter.Invoke("SaveStatusFromApi", null, status, statusLogger, payloadRecordCount);

                Assert.IsNull(actual);
            }


            [TestMethod]
            public void EmployeeImportExporter_SaveStatusFromApi_Calculate_SuccessAndFailureRecords()
            {
                const EmployeeImportStatusEnum status = EmployeeImportStatusEnum.EmployeeImportUpdateBenefits;
                const int payloadRecordCount = 6;
                var statusLogger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager,
                    _retriever, _importHistoryService.Object);

                var log = new Mock<ILog>();

                EmployeeImportExporter employeeImportExporter = new EmployeeImportExporter(log.Object,
                    _storageProvider.Object, new Mock<IImportHistoryService>().Object,
                    ImportConstants.DefaultHttpTimeout, new Mock<INotificationMessageClient>().Object);

                var exporter = new PrivateObject(employeeImportExporter);

                List<EmployeeImportExporter.ValidationReport> validationReports = new List
                    <EmployeeImportExporter.ValidationReport>
                    {
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport1",
                            EmployeeNumber = 1,
                            LastName = "TestLastName1"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport2",
                            EmployeeNumber = 2,
                            LastName = "TestLastName2"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport3",
                            EmployeeNumber = 3,
                            LastName = "TestLastName3"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport4",
                            EmployeeNumber = 4,
                            LastName = "TestLastName4"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport5",
                            EmployeeNumber = 5,
                            LastName = "TestLastName5"
                        }
                    };

                List<EmployeeImportExporter.ValidationTotals> validationTotals = new List
                    <EmployeeImportExporter.ValidationTotals>
                    {
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 2,
                            TotalRecords = 2
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 1,
                            TotalRecords = 1
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 1,
                            TotalRecords = 1
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        }
                    };


                EmployeeImportExporter.EEImportResultContent eeImportResultContent = new EmployeeImportExporter.
                    EEImportResultContent()
                    {
                        EmployeeUpdateCount = 4,
                        HasDuplicateEmployees = false,
                        ValidationReport = validationReports,
                        ValidationTotals = validationTotals
                    };

                EmployeeImportStatusMessage statusMessage = new EmployeeImportStatusMessage();

                exporter.SetFieldOrProperty("_statusMessage", statusMessage);

                exporter.Invoke("SaveStatusFromApi", eeImportResultContent, status, statusLogger, payloadRecordCount);

                Assert.AreEqual(statusMessage.SuccessRecordsCount, 4);
                Assert.AreEqual(statusMessage.FailRecordsCount, 0);

            }

            [TestMethod]
            public void EmployeeImportExporter_SaveStatusFromApi_Calculate_Null_Records()
            {
                const EmployeeImportStatusEnum status = EmployeeImportStatusEnum.EmployeeImportUpdateBenefits;
                const int payloadRecordCount = 6;
                var statusLogger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager,
                    _retriever, _importHistoryService.Object);

                var log = new Mock<ILog>();

                EmployeeImportExporter employeeImportExporter = new EmployeeImportExporter(log.Object,
                    _storageProvider.Object, new Mock<IImportHistoryService>().Object,
                    ImportConstants.DefaultHttpTimeout, new Mock<INotificationMessageClient>().Object);

                var exporter = new PrivateObject(employeeImportExporter);

                List<EmployeeImportExporter.ValidationReport> validationReports = new List
                    <EmployeeImportExporter.ValidationReport>
                    {
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport1",
                            EmployeeNumber = 1,
                            LastName = "TestLastName1"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport2",
                            EmployeeNumber = 2,
                            LastName = "TestLastName2"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport3",
                            EmployeeNumber = 3,
                            LastName = "TestLastName3"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport4",
                            EmployeeNumber = 4,
                            LastName = "TestLastName4"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport5",
                            EmployeeNumber = 5,
                            LastName = "TestLastName5"
                        }
                    };

                List<EmployeeImportExporter.ValidationTotals> validationTotals = new List
                    <EmployeeImportExporter.ValidationTotals>
                    {
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = null,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 2,
                            TotalRecords = 2
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 1,
                            TotalRecords = 1
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = null,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 1,
                            TotalRecords = 2
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 1
                        }
                    };


                EmployeeImportExporter.EEImportResultContent eeImportResultContent = new EmployeeImportExporter.
                    EEImportResultContent()
                    {
                        EmployeeUpdateCount = 4,
                        HasDuplicateEmployees = false,
                        ValidationReport = validationReports,
                        ValidationTotals = validationTotals
                    };

                EmployeeImportStatusMessage statusMessage = new EmployeeImportStatusMessage();

                exporter.SetFieldOrProperty("_statusMessage", statusMessage);

                exporter.Invoke("SaveStatusFromApi", eeImportResultContent, status, statusLogger, payloadRecordCount);

                Assert.AreEqual(statusMessage.SuccessRecordsCount, 4);
                Assert.AreEqual(statusMessage.FailRecordsCount, 2);

            }

            [TestMethod]
            public void EmployeeImportExporter_SaveStatusFromApi_Calculate_Zero_Records()
            {
                const EmployeeImportStatusEnum status = EmployeeImportStatusEnum.EmployeeImportUpdateBenefits;
                const int payloadRecordCount = 6;
                var statusLogger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager,
                    _retriever, _importHistoryService.Object);

                var log = new Mock<ILog>();

                EmployeeImportExporter employeeImportExporter = new EmployeeImportExporter(log.Object,
                    _storageProvider.Object, new Mock<IImportHistoryService>().Object,
                    ImportConstants.DefaultHttpTimeout, new Mock<INotificationMessageClient>().Object);

                var exporter = new PrivateObject(employeeImportExporter);

                List<EmployeeImportExporter.ValidationReport> validationReports = new List
                    <EmployeeImportExporter.ValidationReport>
                    {
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport1",
                            EmployeeNumber = 1,
                            LastName = "TestLastName1"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport2",
                            EmployeeNumber = 2,
                            LastName = "TestLastName2"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport3",
                            EmployeeNumber = 3,
                            LastName = "TestLastName3"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport4",
                            EmployeeNumber = 4,
                            LastName = "TestLastName4"
                        },
                        new EmployeeImportExporter.ValidationReport
                        {
                            Description = "ValidationReport5",
                            EmployeeNumber = 5,
                            LastName = "TestLastName5"
                        }
                    };


                List<EmployeeImportExporter.ValidationTotals> validationTotals = new List
                    <EmployeeImportExporter.ValidationTotals>
                    {
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 1
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        },
                        new EmployeeImportExporter.ValidationTotals
                        {
                            TotalProcessed = 6,
                            TotalRecordsImported = 0,
                            TotalRecords = 0
                        }
                    };


                EmployeeImportExporter.EEImportResultContent eeImportResultContent = new EmployeeImportExporter.
                    EEImportResultContent()
                    {
                        EmployeeUpdateCount = 4,
                        HasDuplicateEmployees = false,
                        ValidationReport = validationReports,
                        ValidationTotals = validationTotals
                    };

                EmployeeImportStatusMessage statusMessage = new EmployeeImportStatusMessage();

                exporter.SetFieldOrProperty("_statusMessage", statusMessage);

                exporter.Invoke("SaveStatusFromApi", eeImportResultContent, status, statusLogger, payloadRecordCount);

                Assert.AreEqual(statusMessage.SuccessRecordsCount, 0);
                Assert.AreEqual(statusMessage.FailRecordsCount, 1);

            }

        }
    }
}