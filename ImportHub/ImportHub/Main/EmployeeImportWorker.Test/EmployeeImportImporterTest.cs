//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using log4net;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Newtonsoft.Json;
//using Paycor.Import.Messaging;
//using Paycor.Import.Status;
//using Paycor.Integration.Adapter;
//using Paycor.Integration.Messaging;
//using Paycor.Import;
//using System.Diagnostics.CodeAnalysis;
//using Paycor.Import.ImportHistory;
//
//namespace EmployeeImportWorker.Test
//{
//    [ExcludeFromCodeCoverage]
//    [TestClass]
//    public class EmployeeImportImporterTest
//    {
//        private const string Record = "N	A	66670	606	HH	05/11/2888	1	Alfred				237 HAYMAN STREET			Novi	MI	48375				201	7	Weekly 2 96912		A					P	Y	M	10.Playwright - 1		N		2080	N						";
//        private const string DuplicateRecord = "N	A	66670	606	HH	05/11/2888	1	Alfred				237 HAYMAN STREET			Novi	MI	48375				201	7	Weekly 2 96912		A					P	Y	M	10.Playwright - 1		N		2080	N						\n\rN	A	66670	606	HH	05/11/2888	1	Alfred				237 HAYMAN STREET			Novi	MI	48375				201	7	Weekly 2 96912		A					P	Y	M	10.Playwright - 1		N		2080	N						\n\rN	A	66670	608	HH	05/11/2888	1	Alfred				237 HAYMAN STREET			Novi	MI	48375				201	7	Weekly 2 96912		A					P	Y	M	10.Playwright - 1		N		2080	N						";
//        private const string BadSequenceNumberRecord = "N	A	66670	606	HH	05/11/2888	R	Alfred				237 HAYMAN STREET			Novi	MI	48375				201	7	Weekly 2 96912		A					P	Y	M	10.Playwright - 1		N		2080	N						";
//        private const string BadEffectiveDateRecord = "N	A	66670	606	HH	13/33/2888	1	Alfred				237 HAYMAN STREET			Novi	MI	48375				201	7	Weekly 2 96912		A					P	Y	M	10.Playwright - 1		N		2080	N						";
//        private const string BadEmployeeNumberRecord = "N	A	66670	E06	HH	13/33/2888	1	Alfred				237 HAYMAN STREET			Novi	MI	48375				201	7	Weekly 2 96912		A					P	Y	M	10.Playwright - 1		N		2080	N						";
//        private const string BadClientId = "N	A	66A70	606	HH	05/11/2888	1	Alfred				237 HAYMAN STREET			Novi	MI	48375				201	7	Weekly 2 96912		A					P	Y	M	10.Playwright - 1		N		2080	N						";
//        private const string ExpectedResult = "{\"RecordType\":\"N\",\"ModificationType\":\"A\",\"ClientId\":\"66670\",\"EmployeeNumber\":\"606\",\"LastName\":\"HH\",\"EffectiveDate\":\"05/11/2888 00:00:00\",\"SeqNumber\":\"1\",\"Field1\":\"Alfred\",\"Field2\":null,\"Field3\":null,\"Field4\":null,\"Field5\":\"237 HAYMAN STREET\",\"Field6\":null,\"Field7\":null,\"Field8\":\"Novi\",\"Field9\":\"MI\",\"Field10\":\"48375\",\"Field11\":null,\"Field12\":null,\"Field13\":null,\"Field14\":\"201\",\"Field15\":\"7\",\"Field16\":\"Weekly 2 96912\",\"Field17\":null,\"Field18\":\"A\",\"Field19\":null,\"Field20\":null,\"Field21\":null,\"Field22\":null,\"Field23\":\"P\",\"Field24\":\"Y\",\"Field25\":\"M\",\"Field26\":\"10.Playwright - 1\",\"Field27\":null,\"Field28\":\"N\",\"Field29\":null,\"Field30\":\"2080\",\"Field31\":\"N\",\"Field32\":null,\"Field33\":null,\"Field34\":null,\"Field35\":null,\"Field36\":null,\"Field37\":null,\"Field38\":null,\"Field39\":null}";
//
//        [TestMethod]
//        public void ImportData_AllGood()
//        {
//            var log = new Mock<ILog>();
//            var storageProvider = new Mock<IStatusStorageProvider>();
//
//
//            var textReader = new StringReader(Record);
//            var fileUploadMessage = new ImportFileUploadMessage { File = "Test.tsv", TransactionId = Guid.NewGuid().ToString() };
//
//            var importer = new PrivateObject(typeof(EmployeeImportImporter), log.Object, storageProvider.Object, new Mock<IImportHistoryService>().Object);
//
//            var actual = importer.Invoke("ImportData", fileUploadMessage, textReader) as FileTranslationData<IDictionary<string, string>>;
//            Assert.IsNotNull(actual);
//            var actualResult = JsonConvert.SerializeObject(actual.Records.FirstOrDefault());
//
//            Assert.AreEqual(actualResult, ExpectedResult);
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(ImportException), "File has an invalid Sequence Number")]
//        public void ImportData_BadSequenceNumber()
//        {
//            var log = new Mock<ILog>();
//            var storageProvider = new Mock<IStatusStorageProvider>();
//            var textReader = new StringReader(BadSequenceNumberRecord);
//            var fileUploadMessage = new ImportFileUploadMessage { File = "Test.tsv", TransactionId = Guid.NewGuid().ToString() };
//            var importer = new PrivateObject(typeof(EmployeeImportImporter), log.Object, storageProvider.Object, new Mock<IImportHistoryService>().Object);
//
//            importer.Invoke("ImportData", fileUploadMessage, textReader);
//
//            log.Object.Error(EmployeeImportResource.EEImportInvalidSequence);
//            log.Verify(e => e.Error(EmployeeImportResource.EEImportInvalidSequence), Times.Once);
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(ImportException), "File has an invalid Effective Date")]
//        public void ImportData_BadEffectiveDate()
//        {
//            var log = new Mock<ILog>();
//            var storageProvider = new Mock<IStatusStorageProvider>();
//            var textReader = new StringReader(BadEffectiveDateRecord);
//            var fileUploadMessage = new ImportFileUploadMessage { File = "Test.tsv", TransactionId = Guid.NewGuid().ToString() };
//            var importer = new PrivateObject(typeof(EmployeeImportImporter), log.Object, storageProvider.Object,new Mock<IImportHistoryService>().Object);
//
//            importer.Invoke("ImportData", fileUploadMessage, textReader);
//
//            log.Object.Error(EmployeeImportResource.EEImportInvalidEffectiveDate);
//            log.Verify(e => e.Error(EmployeeImportResource.EEImportInvalidEffectiveDate), Times.Once);
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(ImportException), "File has an invalid Employee Number")]
//        public void ImportData_BadEmployeeNumber()
//        {
//            var log = new Mock<ILog>();
//            var storageProvider = new Mock<IStatusStorageProvider>();
//            var textReader = new StringReader(BadEmployeeNumberRecord);
//            var fileUploadMessage = new ImportFileUploadMessage { File = "Test.tsv", TransactionId = Guid.NewGuid().ToString() };
//            var importer = new PrivateObject(typeof(EmployeeImportImporter), log.Object, storageProvider.Object, new Mock<IImportHistoryService>().Object);
//
//            importer.Invoke("ImportData", fileUploadMessage, textReader);
//
//            log.Object.Error(EmployeeImportResource.EEImportInvalidEENumber);
//            log.Verify(e => e.Error(EmployeeImportResource.EEImportInvalidEENumber), Times.Once);
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(ImportException), "A bad clientId exists")]
//        public void ImportData_BadClientId()
//        {
//            var log = new Mock<ILog>();
//            var storageProvider = new Mock<IStatusStorageProvider>();
//            var textReader = new StringReader(BadClientId);
//            var fileUploadMessage = new ImportFileUploadMessage { File = "Test.tsv", TransactionId = Guid.NewGuid().ToString() };
//            var importer = new PrivateObject(typeof(EmployeeImportImporter), log.Object, storageProvider.Object,new Mock<IImportHistoryService>().Object);
//
//            importer.Invoke("ImportData", fileUploadMessage, textReader);
//
//            log.Object.Error(EmployeeImportResource.EEImportBadClientId);
//            log.Verify(e => e.Error(EmployeeImportResource.EEImportBadClientId), Times.Once);
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(ImportException), "Exact duplicate record headers found at Employee 606 for Record Type N.")]
//        public void ImportData_DuplicateRecords()
//        {
//            var log = new Mock<ILog>();
//            var storageProvider = new Mock<IStatusStorageProvider>();
//            var textReader = new StringReader(DuplicateRecord);
//            var fileUploadMessage = new ImportFileUploadMessage { File = "Test.tsv", TransactionId = Guid.NewGuid().ToString() };
//            var importer = new PrivateObject(typeof(EmployeeImportImporter), log.Object, storageProvider.Object, new Mock<IImportHistoryService>().Object);
//
//            importer.Invoke("ImportData", fileUploadMessage, textReader);
//
//            log.Object.Error("Exact duplicate record headers found at Employee 606 for Record Type N.");
//            log.Verify(e => e.Error("Exact duplicate record headers found at Employee 606 for Record Type N."), Times.Once);
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(ArgumentNullException), "Expected exception for wrong type passed into ImportData method.")]
//        public void ImportData_FileUploadMessage_Is_Wrong_Type()
//        {
//            var log = new Mock<ILog>();
//            var storageProvider = new Mock<IStatusStorageProvider>();
//            var textReader = new StringReader(DuplicateRecord);
//            var fileUploadMessage = new FileUploadMessage { File = "Test.tsv" };
//            var importer = new PrivateObject(typeof(EmployeeImportImporter), log.Object, storageProvider.Object, new Mock<IImportHistoryService>().Object);
//
//            importer.Invoke("ImportData", fileUploadMessage, textReader);
//        }
//    }
//}
