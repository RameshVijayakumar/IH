using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.Employee.Status;
using Paycor.Import.ImportHistory;
using Paycor.Import.Status;

namespace EmployeeImportWorker.Test
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EmployeeImportLoggerTest
    {
        private const string Reporter = "ExportStatus";
        private const string FileName = "TestFileName";

        private static Mock<IStatusStorageProvider> _storageProvider;
        private static StatusManager<EmployeeImportStatusMessage> _manager;
        private static ImportStatusReceiver<EmployeeImportStatusMessage> _receiver;
        private static ImportStatusRetriever<EmployeeImportStatusMessage> _retriever;
        private static Mock<IImportHistoryService> _importHistoryService;
        private static Mock<ILog> _log;

        private static readonly List<StatusMessage> StatusMessageStore = new List<StatusMessage>();

        private static void CreateOrUpdate(StatusMessage message)
        {
            if (StatusMessageStore.Count > 0)
            {
                var index = StatusMessageStore.FindIndex(x => x.Reporter == Reporter && x.Key == "TestFileName");

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

        [TestCleanup]
        public void Cleanup()
        {
            StatusMessageStore.Clear();
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestInitialize]
        public void Setup()
        {
            _storageProvider = new Mock<IStatusStorageProvider>();
            _storageProvider.Setup(e => e.StoreStatus(It.IsAny<StatusMessage>())).Callback<StatusMessage>(CreateOrUpdate);
            _storageProvider.Setup(e => e.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>())).Returns(() => StatusMessageStore.SingleOrDefault(x => x.Reporter == Reporter && x.Key == FileName));

            _receiver = new ImportStatusReceiver<EmployeeImportStatusMessage>(Reporter);
            _manager = new StatusManager<EmployeeImportStatusMessage>(_receiver, _storageProvider.Object);
            _retriever = new ImportStatusRetriever<EmployeeImportStatusMessage>(_storageProvider.Object, Reporter);
            _importHistoryService = new Mock<IImportHistoryService>();
            _log = new Mock<ILog>();
            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmployeeImportLoggerTest_RetrieveMessage_IsNull()
        {
            var storageProvider = new Mock<IStatusStorageProvider>();
            storageProvider.Setup(e => e.StoreStatus(It.IsAny<StatusMessage>())).Callback<StatusMessage>(CreateOrUpdate);
            storageProvider.Setup(e => e.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);
            var receiver = new ImportStatusReceiver<EmployeeImportStatusMessage>(Reporter);
            var manager = new StatusManager<EmployeeImportStatusMessage>(receiver, storageProvider.Object);

            var retriever = new ImportStatusRetriever<EmployeeImportStatusMessage>(storageProvider.Object, Reporter);

            
            var logger = new EmployeeImportStatusLogger(FileName, receiver, storageProvider.Object, manager, retriever, _importHistoryService.Object);

            var message = new EmployeeImportStatusMessage
            {
                FileName = FileName,
                SuccessRecordsCount = 1,
                FailRecordsCount = 1,
                WarnRecordCount = 1,
            };

            logger.LogMessage(message);
            logger.RetrieveMessage();
        }

        [TestMethod]
        public void EmployeeImportLoggerTest_LogMessage()
        {
            var logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);

            var message = new EmployeeImportStatusMessage
            {
                FileName = FileName,
                SuccessRecordsCount = 1,
                FailRecordsCount = 1,
                Status = EmployeeImportStatusEnum.ImportFileData
            };

            var detail = new EmployeeImportStatusDetail
            {
                EmployeeName = "Kelly Klein",
                EmployeeNumber = "123456",
                Issue = "Blah",
                RecordUploaded = false,                
            };
            message.StatusDetails.Add(detail);

            logger.LogMessage(message);

            var addedMessage = StatusMessageStore.Single();
            _storageProvider.Verify(r => r.StoreStatus(It.IsAny<StatusMessage>()), Times.Once);

            var expected = JsonConvert.SerializeObject(message);

            Assert.AreEqual(expected, addedMessage.Status);
        }

        [TestMethod]
        public void EmployeeImportLoggerTest_LogMessage_AddDetail()
        {
            var logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);

            var message = new EmployeeImportStatusMessage
            {
                FileName = FileName,
                SuccessRecordsCount = 1,
                FailRecordsCount = 1,
                Status = EmployeeImportStatusEnum.ImportFileData
            };
           
            logger.LogMessage(message);

            var detail = new EmployeeImportStatusDetail
            {
                EmployeeName = "Kelly Klein",
                EmployeeNumber = "123456",
                Issue = "Blah",
                RecordUploaded = false                
            };

            logger.LogDetail(detail);
            
            var addedMessage = StatusMessageStore.Single();
            _storageProvider.Verify(r => r.StoreStatus(It.IsAny<StatusMessage>()), Times.AtLeastOnce);
            _storageProvider.Verify(r => r.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);

            message.StatusDetails.Add(detail);
            var expected = JsonConvert.SerializeObject(message);

            Assert.AreEqual(expected, addedMessage.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmployeeImportLoggerTest_LogMessage_AddDetail_NullMessage()
        {
            var logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);
            
            var detail = new EmployeeImportStatusDetail
            {
                EmployeeName = "Kelly Klein",
                EmployeeNumber = "123456",
                Issue = "Blah",
                RecordUploaded = false                
            };

            logger.LogDetail(detail);
        }

        [TestMethod]
        public void EmployeeImportLoggerTest_IncrementSuccess()
        {
            var logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);

            var message = new EmployeeImportStatusMessage
            {
                FileName = FileName,
                SuccessRecordsCount = 1,
                FailRecordsCount = 1
            };

            var detail = new EmployeeImportStatusDetail
            {
                EmployeeName = "Kelly Klein",
                EmployeeNumber = "123456",
                Issue = "Blah",
                RecordUploaded = false                
            };
            message.StatusDetails.Add(detail);

            logger.LogMessage(message);
            logger.IncrementSuccess();

            var addedMessage = StatusMessageStore.Single();
            _storageProvider.Verify(r => r.StoreStatus(It.IsAny<StatusMessage>()), Times.AtLeastOnce);
            _storageProvider.Verify(r => r.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);

            message.SuccessRecordsCount = 2;            
            var expected = JsonConvert.SerializeObject(message);

            Assert.AreEqual(expected, addedMessage.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmployeeImportLoggerTest_IncrementSuccess_NullMessage()
        {

            var logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);
           
            logger.IncrementSuccess();            
        }

        [TestMethod]
        public void EmployeeImportLoggerTest_IncrementWarn()
        {
            var logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);

            var message = new EmployeeImportStatusMessage
            {
                FileName = FileName,
                SuccessRecordsCount = 1,
                FailRecordsCount = 1,
                WarnRecordCount = 1,
            };

            var detail = new EmployeeImportStatusDetail
            {
                EmployeeName = "Kelly Klein",
                EmployeeNumber = "123456",
                Issue = "Blah",
                RecordUploaded = false
            };
            message.StatusDetails.Add(detail);

            logger.LogMessage(message);
            logger.IncrementWarn();

            var addedMessage = StatusMessageStore.Single();
            _storageProvider.Verify(r => r.StoreStatus(It.IsAny<StatusMessage>()), Times.AtLeastOnce);
            _storageProvider.Verify(r => r.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);

            message.WarnRecordCount = 2;
            var expected = JsonConvert.SerializeObject(message);

            Assert.AreEqual(expected, addedMessage.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmployeeImportLogerTest_IncrementWarn_NullMessage()
        {
            var logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);
            logger.IncrementWarn();
        }

        [TestMethod]
        public void EmployeeImportLoggerTest_IncrementFailure()
        {
            var logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);

            var message = new EmployeeImportStatusMessage
            {
                FileName = FileName,
                SuccessRecordsCount = 1,
                FailRecordsCount = 1
            };

            var detail = new EmployeeImportStatusDetail
            {
                EmployeeName = "Kelly Klein",
                EmployeeNumber = "123456",
                Issue = "Blah",
                RecordUploaded = false                
            };
            message.StatusDetails.Add(detail);

            logger.LogMessage(message);
            logger.IncrementFail();

            var addedMessage = StatusMessageStore.Single();
            _storageProvider.Verify(r => r.StoreStatus(It.IsAny<StatusMessage>()), Times.AtLeastOnce);
            _storageProvider.Verify(r => r.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);

            message.FailRecordsCount = 2;
            var expected = JsonConvert.SerializeObject(message);

            Assert.AreEqual(expected, addedMessage.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmployeeImportLoggerTest_IncrementFailure_NullMessage()
        {
            EmployeeImportStatusLogger logger = new EmployeeImportStatusLogger(FileName, _receiver, _storageProvider.Object, _manager, _retriever, _importHistoryService.Object);

            logger.IncrementFail();
        }
    }
}
