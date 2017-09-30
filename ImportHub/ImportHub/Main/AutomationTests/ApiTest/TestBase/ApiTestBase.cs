using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.ImportHubTest.Common;
using static Paycor.Import.ImportHubTest.Common.Utils;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    [TestClass]
    public class ApiTestBase
    {
        public TestContext TestContext { get; set; }

        public ApiTestClient TestClient { get; private set; }

        public  const TestDeployment Deployment = null;

        public ApiTestBase()
        {
            TestClient = new ApiTestClient();
        }

        [ClassInitialize()]
        public void ApiTestBaseInitialize(TestContext testContext)
        {
            Log("Initialize TestClient");
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            Log($"Starting {TestContext.TestName}");
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            Log($"Cleanup {TestContext.TestName}");
        }
    }

    public class TestDeployment
    {
        public  string TestFile { get; private set; }

        public TestDeployment(string testFile)
        {
            TestFile = testFile;
        }
    }
}
