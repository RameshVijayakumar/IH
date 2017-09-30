using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.ImportHistory;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.Messaging;
using Paycor.Import.Status;
using System.Threading.Tasks;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.Http;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ReportProcessorTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void SendEventTest_Success()
        {
            var log = new Mock<ILog>();
            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();


            var importHistoryService = new Mock<IImportHistoryService>();
            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();
            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);

            reportProcessor.SendEvent(new MappedFileImportStatusMessage());
        }

        [TestMethod]
        public void SaveFailedDataTest_NoFailed_Records_Success()
        {
            var log = new Mock<ILog>();
            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();


            var importHistoryService = new Mock<IImportHistoryService>();
            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();
            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);

            reportProcessor.SaveFailedData("1212", new List<FailedRecord>(), 1);
        }

        [TestMethod]
        public void SaveFailedDataTest_Handles_Exception_Properly()
        {
            var log = new Mock<ILog>();
            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();


            var importHistoryService = new Mock<IImportHistoryService>();
            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();
            failedRecordFormatter.Setup(t => t.GenerateXlsxData
            (It.IsAny<IList<FailedRecord>>())
            ).Throws(new Exception());

            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);

            reportProcessor.SaveFailedData("1212", new List<FailedRecord>
            {
                new FailedRecord()
            }, 1);
        }

        [TestMethod]
        public void MultiSheet_SaveFailedDataTest_NoFailed_Records_Success()
        {
            var log = new Mock<ILog>();
            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();


            var importHistoryService = new Mock<IImportHistoryService>();
            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();
            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);

            reportProcessor.SaveFailedData("1", new Dictionary<string, IList<FailedRecord>>(),
                1);
        }

        [TestMethod]
        public void MultiSheet_SaveFailedDataTest_Success()
        {
            var log = new Mock<ILog>();
            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();


            var importHistoryService = new Mock<IImportHistoryService>();
            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();

            failedRecordFormatter.Setup(t => t.GenerateXlsxData
                    (It.IsAny<IList<FailedRecord>>())
            ).Returns(new byte[3]);

            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);

            reportProcessor.SaveFailedData("1212",
                new Dictionary<string, IList<FailedRecord>>
                {
                    ["1"] = new List<FailedRecord>()
                },

                1);
        }

        [TestMethod]
        public void MultiSheet_SaveFailedDataTest_Handles_Exception_Properly()
        {
            var log = new Mock<ILog>();
            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();


            var importHistoryService = new Mock<IImportHistoryService>();
            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();

            failedRecordFormatter.Setup(t => t.GenerateXlsxData
            (It.IsAny<IDictionary<string, IList<FailedRecord>>>())
            ).Throws(new Exception());

            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);

            reportProcessor.SaveFailedData("1212",
                new Dictionary<string, IList<FailedRecord>>
                {
                    ["1"] = new List<FailedRecord>()
                },

                1);
        }


        [TestMethod]
        public async Task SaveStatusFromApi_Success()
        {
            var log = new Mock<ILog>();
            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();



            var storageProvider = new Mock<IStatusStorageProvider>();
            var receiver = new ImportStatusReceiver<MappedFileImportStatusMessage>("test");
            var manager = new StatusManager<MappedFileImportStatusMessage>(receiver, storageProvider.Object);
            var retriever = new ImportStatusRetriever<MappedFileImportStatusMessage>(storageProvider.Object,
                "test");
            var importHistoryService = new Mock<IImportHistoryService>();
            var statusLogger = new MappedFileImportStatusLogger("1", receiver,
                storageProvider.Object, manager, retriever, importHistoryService.Object);


            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();
            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);

           await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
                statusLogger,
                new List<ErrorResultData>());

           await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
            statusLogger,
            new List<ErrorResultData> { 
            new ErrorResultData
            {
                ErrorResponse = new ErrorResponse
                {
                    Source = new Dictionary<string, string>()
                }
            }
            });

            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
            statusLogger,
            new List<ErrorResultData> {
            new ErrorResultData
            {
                ErrorResponse = new ErrorResponse
                {
                    Source = null
                }
            }});

            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
            statusLogger,
             new List<ErrorResultData> {
            new ErrorResultData
            {
                ErrorResponse = new ErrorResponse
                {
                    Source = null,
                },
                HttpExporterResult = new HttpExporterResult
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Accepted
                    }
                }
            }});

            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
            statusLogger,
            null);

            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
           statusLogger,
            new List<ErrorResultData> {
           new ErrorResultData
           {
               ErrorResponse = new ErrorResponse
               {
                   Source = null,
               },
               HttpExporterResult = new HttpExporterResult
               {
                   Response = new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.Forbidden
                   }
               }
           }});

           await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
          statusLogger,
           new List<ErrorResultData> {
          new ErrorResultData
          {
              ErrorResponse = new ErrorResponse
              {
                  Source = new Dictionary<string, string>
                  {
                      {"errorkey","errorvalue"}
                  },
              },
              HttpExporterResult = new HttpExporterResult
              {
                  Response = new HttpResponseMessage
                  {
                      StatusCode = HttpStatusCode.Forbidden
                  }
              }
          }});

        }


        [TestMethod]
        public async Task UpdateStatusRecordCount_Success()
        {
            var log = new Mock<ILog>();
            var eventMessageClient = new Mock<ICloudMessageClient<FileImportEventMessage>>();



            var storageProvider = new Mock<IStatusStorageProvider>();
            var receiver = new ImportStatusReceiver<MappedFileImportStatusMessage>("test");
            var manager = new StatusManager<MappedFileImportStatusMessage>(receiver, storageProvider.Object);
            var retriever = new ImportStatusRetriever<MappedFileImportStatusMessage>(storageProvider.Object,
                "test");
            var importHistoryService = new Mock<IImportHistoryService>();
            var statusLogger = new MappedFileImportStatusLogger("1", receiver,
                storageProvider.Object, manager, retriever, importHistoryService.Object);


            var failedRecordFormatter = new Mock<IXlsxRecordFormatter<FailedRecord>>();
            var errorFormatter = new Mock<IErrorFormatter>();
            var reportProcessor = new ReportProcessor(eventMessageClient.Object,
                log.Object, importHistoryService.Object, failedRecordFormatter.Object, errorFormatter.Object);


            await reportProcessor.UpdateStatusRecordCountAsync(new MappedFileImportStatusMessage(), statusLogger, 4, 4);
            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
                statusLogger,
                new List<ErrorResultData>());

            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
            statusLogger,
            new List<ErrorResultData> { 
            new ErrorResultData
            {
                ErrorResponse = new ErrorResponse
                {
                    Source = new Dictionary<string, string>()
                }
            }});

            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
            statusLogger,
             new List<ErrorResultData> {
            new ErrorResultData
            {
                ErrorResponse = new ErrorResponse
                {
                    Source = null
                }
            }});

            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
            statusLogger,
            new List<ErrorResultData> {
            new ErrorResultData
            {
                ErrorResponse = new ErrorResponse
                {
                    Source = null,
                },
                HttpExporterResult = new HttpExporterResult
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Accepted
                    }
                }
            }});

           await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
            statusLogger,
            null);

            await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
           statusLogger,
           new List<ErrorResultData> {
           new ErrorResultData
           {
               ErrorResponse = new ErrorResponse
               {
                   Source = null,
               },
               HttpExporterResult = new HttpExporterResult
               {
                   Response = new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.Forbidden
                   }
               }
           }});

           await reportProcessor.SaveStatusFromApiAsync(new MappedFileImportStatusMessage(),
          statusLogger,
          new List<ErrorResultData> {
          new ErrorResultData
          {
              ErrorResponse = new ErrorResponse
              {
                  Source = new Dictionary<string, string>
                  {
                      {"errorkey","errorvalue"}
                  },
              },
              HttpExporterResult = new HttpExporterResult
              {
                  Response = new HttpResponseMessage
                  {
                      StatusCode = HttpStatusCode.Forbidden
                  }
              }
          }});

        }

    }
}
