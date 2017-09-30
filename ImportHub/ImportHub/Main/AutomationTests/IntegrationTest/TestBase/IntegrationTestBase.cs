using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.IntegrationTest.TestBase
{
    [TestClass]
    [DeploymentItem(@"TestFiles", "TestFiles")]
    [DeploymentItem("CustomMaps", "CustomMaps")]
    public class IntegrationTestBase
    {
        private const string TestFileDir = "TestFiels";
        private const string CustomMapDir = "Maps";
        public TestContext TestContext { get; set; }
        public IntegrationTestClient TestClient { get; private set; }
        public  const TestDeployment Deployment = null;

        public void TryUploadFile(string filename, string[] mapNames = null, bool useCustomMap = false)
        {
            if (useCustomMap)
                TestClient.UploadFileUsingCustomMap(filename, GetCustomMaps(mapNames));
            else
                TestClient.UploadFileUsingRegisteredMap(GetTestFile(filename), mapNames);
        }

        public string GetTestFile(string fileName) =>
            Path.Combine(TestContext.TestDeploymentDir, TestFileDir, fileName);

        public string[] GetCustomMaps(string[] mapNames) => 
            mapNames.Select(name => File.ReadAllText(Path.Combine(TestContext.TestDeploymentDir, CustomMapDir, name))).ToArray();

        public IntegrationTestBase()
        {
            TestClient = new IntegrationTestClient();
        }

        [ClassInitialize()]
        public void ApiTestBaseInitialize(TestContext testContext)
        {
            Utils.Log("Initialize TestClient");
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            Utils.Log($"Starting {TestContext.TestName}");
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            Utils.Log($"Cleanup {TestContext.TestName}");
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
