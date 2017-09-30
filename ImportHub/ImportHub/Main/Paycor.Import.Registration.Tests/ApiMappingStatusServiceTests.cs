using System;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Mapping;
using Moq;
using Paycor.Import.Azure;

namespace Paycor.Import.Registration.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApiMappingStatusServiceTests
    {
        public static Mock<IDocumentDbRepository<ApiMappingStatusInfo>> MockRepo { get; set; }

        [TestClass]
        public class ConstructorTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApiMappingStatusServiceCtor_Ensure()
            {
                var service = new ApiMappingStatusService(null);
                Assert.IsNull(service);
            }

            [TestMethod]
            public void ApiMappingStatusServiceCtor_Threshold()
            {
                var repo = new Mock<IDocumentDbRepository<ApiMappingStatusInfo>>();
                var service = new ApiMappingStatusService(repo.Object);
                var expected = 100;
                Assert.AreEqual(expected, service.RegistrationThreshold);
            }
        }

        [TestClass]
        public class UpdateStatusInfoTests
        {
            public UpdateStatusInfoTests()
            {
                MockRepo = new Mock<IDocumentDbRepository<ApiMappingStatusInfo>>();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApiMappingStatusInfoEnsured()
            {
                var service = new ApiMappingStatusService(MockRepo.Object);
                service.UpdateApiStatusInfo(null);
            }

            [TestMethod]
            public void UpdateApiStatusInfoWorks()
            {
                var service = new ApiMappingStatusService(MockRepo.Object);
                service.UpdateApiStatusInfo(new ApiMappingStatusInfo
                {
                    DocUrl = "non empty",
                });
                MockRepo.Verify(e => e.UpdateItemAsync(It.IsAny<string>(), It.IsAny<ApiMappingStatusInfo>()), Times.Once);
            }
        }

        [TestClass]
        public class CreateApiStatusInfoTests
        {
            public CreateApiStatusInfoTests()
            {
                MockRepo = new Mock<IDocumentDbRepository<ApiMappingStatusInfo>>();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void DocUrlEnsured()
            {
                var service = new ApiMappingStatusService(MockRepo.Object);
                service.CreateApiStatusInfo(null);
            }

            // TEST DOESN'T WORK - IT HANGS MSTEST AND I DON'T KNOW WHY :-(
            //    [TestMethod]
            //    public void CreateApiStatusInfoWorks()
            //    {
            //        var service = new ApiMappingStatusService(MockRepo.Object);
            //        MockRepo.Setup(e => e.CreateItemAsync(It.IsAny<ApiMappingStatusInfo>()))
            //            .Returns(new Task<Document>(() => new Document
            //            {
            //                Id = "test"
            //            }));

            //        service.CreateApiStatusInfo("non-empty");
            //        MockRepo.Verify(e => e.CreateItemAsync(It.IsAny<ApiMappingStatusInfo>()), Times.Once);
            //    }
            //
        }

        [TestClass]
        public class GetApiStatusInfoTests
        {
            public GetApiStatusInfoTests()
            {
                MockRepo = new Mock<IDocumentDbRepository<ApiMappingStatusInfo>>();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void GetApiStatusInfoEnsuresDocUrl()
            {
                var service = new ApiMappingStatusService(MockRepo.Object);
                service.GetApiStatusInfo(null);
            }

            [TestMethod]
            public void GetApiStatusInfoWorks()
            {
                var service = new ApiMappingStatusService(MockRepo.Object);
                service.GetApiStatusInfo("test url");
                MockRepo.Verify(e => e.GetItem(It.IsAny<Expression<Func<ApiMappingStatusInfo, bool>>>()), Times.Once);
            }
        }

        [TestClass]
        public class IsRecentlyProcessedTests
        {
            public IsRecentlyProcessedTests()
            {
                MockRepo = new Mock<IDocumentDbRepository<ApiMappingStatusInfo>>();
            }

            [TestMethod]
            public void IsRecentlyProcessedReturnsFalseIfNull()
            {
                var service = new ApiMappingStatusService(MockRepo.Object);
                var actual = service.IsRecentlyProcessed(null);
                Assert.IsFalse(actual);
            }

            [TestMethod]
            public void IsRecentlyProcessedReturnsFalseIfNullSub()
            {
                var service = new ApiMappingStatusService(MockRepo.Object);
                var actual = service.IsRecentlyProcessed(new ApiMappingStatusInfo());
                Assert.IsFalse(actual);
            }

            [TestMethod]
            public void IsRecentlyProcessedReturnsTrue()
            {
                var service = new ApiMappingStatusService(MockRepo.Object);
                var actual = service.IsRecentlyProcessed(new ApiMappingStatusInfo
                {
                    DocUrl = "test url",
                    LastRegistered = DateTime.Now
                });
                Assert.IsTrue(actual);
            }
        }
    }
}
