using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.Azure;
using Paycor.Import.Employee.ImportHistory;
using Paycor.Import.ImportHistory;
using Paycor.Import.Status;

namespace Paycor.Import.Employee.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ImportHistoryServiceTests
    {
        [TestClass]
        public class DeleteImportHistoryByIdTests
        {
            private readonly Mock<IStorageProvider> _mockStorageProvider = new Mock<IStorageProvider>();
            private readonly Mock<IStatusStorageProvider> _mockStatusStorageProvider = new Mock<IStatusStorageProvider>();
            private readonly Mock<ILog> _mockLog = new Mock<ILog>();
            private readonly Mock<IDocumentDbRepository<ImportHistoryMessage>> _mockRepo = new Mock<IDocumentDbRepository<ImportHistoryMessage>>();

            [TestCleanup]
            public void TestCleanup()
            {
                CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
            }

            [TestMethod]
            public async Task VerifyItemFound()
            {
                _mockRepo.Setup(x => x.GetItem(It.IsAny<Expression<Func<ImportHistoryMessage, bool>>>()))
                    .Returns(() => new ImportHistoryMessage
                    {
                        TransactionId = "1234",
                        ClientId = "1234",
                        FileName = "1234",
                        ImportDate = DateTime.Now,
                        ImportType = "Bogus",
                        User = "alan"
                    });

                var historyService = new ImportHistoryService(
                    _mockStorageProvider.Object,
                    _mockStatusStorageProvider.Object,
                    _mockLog.Object,
                    _mockRepo.Object);
                Assert.IsNotNull(historyService);
                var actual = await historyService.DeleteImportHistory("alan", "1234");
                Assert.IsTrue(actual);

                _mockStorageProvider.Verify(x => x.DeleteTextFromBlobAsync(
                    It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Once);
                _mockStatusStorageProvider.Verify(x => x.DeleteStatusAsync(
                    It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Once);
            }

            [TestMethod]
            public async Task VerifyItemNotFound()
            {
                _mockRepo.Setup(x => x.GetItem(It.IsAny<Expression<Func<ImportHistoryMessage, bool>>>()))
                    .Returns(() => null);

                var historyService = new ImportHistoryService(
                    _mockStorageProvider.Object,
                    _mockStatusStorageProvider.Object,
                    _mockLog.Object,
                    _mockRepo.Object);
                Assert.IsNotNull(historyService);
                var actual = await historyService.DeleteImportHistory("jimbo", "98392");
                Assert.IsFalse(actual);
            }
        }
    }
}
