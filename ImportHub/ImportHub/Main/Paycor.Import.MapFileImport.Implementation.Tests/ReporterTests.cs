using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.Http;
using Paycor.Import.ImportHistory;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.Messaging;
using Paycor.Import.Status;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ReporterTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void Report_Test()
        {
            var importHistoryService = new Mock<IImportHistoryService>();
            var log = new Mock<ILog>();
            var statusMessage = new StatusMessage
            {
                Key = "testkey",
                Reporter = "testreporter",
                Status = File.ReadAllText("Json\\MappedFileImportstatus.json")
            };

            var storageProvider = new Mock<IStatusStorageProvider>();
            storageProvider.Setup(t => t.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(statusMessage);

            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();

            var calculator = new Mock<ICalculate>();
            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();
            var provideClientData = new Mock<IProvideClientData<MapFileImportResponse>>();
            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);
            var notificationMessageClient = new Mock<INotificationMessageClient>();

            var context = new ImportContext
            {
                HasHeader = true,

                CallApiInBatch = true,
                IsMultiSheetImport = true,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = "12345678-1234-1234-1234-123456789012",
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                }
            };
            var reporter = new Reporter.Reporter(log.Object, storageProvider.Object, importHistoryService.Object,
                calculator.Object, reportProcessor, provideClientData.Object, notificationMessageClient.Object);
            reporter.Initialize(context);
            reporter.ReportAsync(StepNameEnum.Builder, new BuildDataSourceResponse());
            reporter.ReportAsync(StepNameEnum.Chunker, new MapFileImportResponse());
            reporter.ReportAsync(StepNameEnum.Sender, new PayloadSenderResponse
            {
                ErrorResultDataItems = new List<ErrorResultData>
                {
                    new ErrorResultData
                    {
                        ErrorResponse = new ErrorResponse(),
                        HttpExporterResult = new HttpExporterResult
                        {
                            Response = new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.Accepted
                            }
                        },
                        ImportType = "Employee Api"
                    }
                }
            });

            reporter.ReportAsync(StepNameEnum.Sender, new PayloadSenderResponse
            {
                Status = Status.Failure,
                ErrorResultDataItems = new List<ErrorResultData>
                {
                    new ErrorResultData
                    {
                        ErrorResponse = new ErrorResponse(),
                        HttpExporterResult = new HttpExporterResult
                        {
                            Response = new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.BadGateway
                            }
                        },
                        ImportType = "Employee Api"
                    }
                }
            });
            reporter.ReportCompletionAsync();
        }

        [TestMethod]
        public void Report_Cancel_Test()
        {
            var importHistoryService = new Mock<IImportHistoryService>();
            var log = new Mock<ILog>();
            var statusMessage = new StatusMessage
            {
                Key = "testkey",
                Reporter = "testreporter",
                Status = File.ReadAllText("Json\\MappedFileImportstatus.json")
            };

            var storageProvider = new Mock<IStatusStorageProvider>();
            storageProvider.Setup(t => t.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(statusMessage);

            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();

            var calculator = new Mock<ICalculate>();
            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();
            var provideClientData = new Mock<IProvideClientData<MapFileImportResponse>>();
            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);
            var notificationMessageClient = new Mock<INotificationMessageClient>();

            var context = new ImportContext
            {
                HasHeader = true,

                CallApiInBatch = true,
                IsMultiSheetImport = true,
                ChunkSize = 5,
                ChunkNumber = 1,
                MasterSessionId = "1",
                TransactionId = "1",
                Endpoints = new Dictionary<HtmlVerb, string>
                {
                    {HtmlVerb.Post, "http://localhost/employeeservice/v2/employees"}

                }
            };
            var reporter = new Reporter.Reporter(log.Object, storageProvider.Object, importHistoryService.Object,
                calculator.Object, reportProcessor, provideClientData.Object, notificationMessageClient.Object);
            reporter.Initialize(context);
            reporter.ReportAsync(StepNameEnum.Chunker, new MapFileImportResponse
            {
                TotalChunks = 1,
                TotalRecordsCount = 5
            });
            reporter.ReportAsync(StepNameEnum.Builder, new BuildDataSourceResponse());

            reporter.ReportAsync(StepNameEnum.Preparer, new PreparePayloadResponse
            {
                ErrorResultDataItems = new List<ErrorResultData>
                {
                    new ErrorResultData
                    {
                        ErrorResponse = new ErrorResponse(),
                        HttpExporterResult = new HttpExporterResult
                        {
                            Response = new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.Accepted
                            }
                        },
                        ImportType = "Employee Api"
                    }
                }
            });

            reporter.ReportAsync(StepNameEnum.Sender, new PayloadSenderResponse
            {
                Status = Status.Failure,
                ErrorResultDataItems = new List<ErrorResultData>
                {
                    new ErrorResultData
                    {
                        ErrorResponse = new ErrorResponse(),
                        HttpExporterResult = new HttpExporterResult
                        {
                            Response = new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.BadGateway
                            }
                        },
                        ImportType = "Employee Api"
                    }
                }
            });
            reporter.CanceledReport();
        }
    }
}
