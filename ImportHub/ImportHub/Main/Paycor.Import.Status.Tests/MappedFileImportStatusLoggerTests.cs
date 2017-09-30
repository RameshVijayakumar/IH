using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.ImportHistory;

namespace Paycor.Import.Status.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappedFileImportStatusLoggerTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }
        [TestMethod]
        public void LogMessage_Success()
        {
            var storageProvider = new Mock<IStatusStorageProvider>();
            var receiver = new ImportStatusReceiver<MappedFileImportStatusMessage>("test");
            var manager = new StatusManager<MappedFileImportStatusMessage>(receiver, storageProvider.Object);
            var retriever = new ImportStatusRetriever<MappedFileImportStatusMessage>(storageProvider.Object, 
                "test");
            var importHistoryService = new Mock<IImportHistoryService>();
            var statusLogger = new MappedFileImportStatusLogger("1", receiver, 
                storageProvider.Object, manager, retriever, importHistoryService.Object);

            statusLogger.LogMessage(new MappedFileImportStatusMessage());
            statusLogger.LogMessage(new MappedFileImportStatusMessage
            {
                Status = MappedFileImportStatusEnum.Queued
            });
            statusLogger.LogMessage(new MappedFileImportStatusMessage
            {
                Status = MappedFileImportStatusEnum.ImportFileData
            });
            statusLogger.LogMessage(new MappedFileImportStatusMessage
            {
                Status = MappedFileImportStatusEnum.PrevalidationFailure
            });

        }
    }
}
