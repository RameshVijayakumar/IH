using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.FileType;
using Paycor.Import.ImportHistory;
using Paycor.Import.JsonFormat;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using Paycor.Import.Service.FileType;
using Paycor.Import.Status;

namespace Paycor.Import.Service.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappedFileTypeHandlerTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void CancelTest()
        {
            var importHistoryService = new Mock<IImportHistoryService>();
            var log = new Mock<ILog>();
            var fileStorage = new Mock<IStoreFile>();
            var fileProcessor = new Mock<ICloudMessageClient<Messaging.FileUploadMessage>>();
            var fieldMapper = new Mock<IFieldMapper>();
            var formatJson = new Mock<IFormatJson>();
            var cancelTokenStorage = new Mock<IStoreData<ImportCancelToken>>();
            var statusMessage = new StatusMessage
            {
                Key = "testkey",
                Reporter = "testreporter",
                Status = File.ReadAllText("Json\\MappedFileImportstatus.json")
            };

            var storageProvider = new Mock<IStatusStorageProvider>();
            storageProvider.Setup(t => t.RetrieveStatus(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(statusMessage);
            const string sbConnection = "DefaultEndpointsProtocol=https;AccountName=aanwarstorage;AccountKey=+v6GpS1JNFrIhIM4tZ7u4LUvD1QLUyAcXEZzoOagZvj/KXUvIhqYIksu1ehfkpMezDKuD95xYNA9jbLzLR0UYw==";
            const string user = "93b48f53-8876-e511-8893-005056bd7869";
            const string transactionId = "caffe6f5-500f-40e2-b5e9-189e28752b00";
            var mappedFileTypeHandler = new MappedFileTypeHandler(log.Object, fileStorage.Object, fileProcessor.Object,
                importHistoryService.Object,
                fieldMapper.Object, sbConnection, formatJson.Object, cancelTokenStorage.Object,
                storageProvider.Object);

            mappedFileTypeHandler.Cancel(user, transactionId, DateTime.Now);
        }

        [TestMethod]
        public void MaxNumberOfColumnsTest()
        {
            var data = new List<string>
            {
                @"C,102,105201,3364984172,Active,1.15,1234,12",
                @"C,102,105201,3364984172,Active,1.15,1234,12"
            };


            var maxCount = FileTypeRecognizer.GetMaxNumberOfColumns(data, ',');
            Assert.AreEqual(8, maxCount);

        }

    }
}
