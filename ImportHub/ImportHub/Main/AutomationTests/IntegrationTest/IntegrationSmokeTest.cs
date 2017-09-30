using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.ImportHubTest.IntegrationTest.TestBase;

namespace Paycor.Import.ImportHubTest.IntegrationTest
{
    [TestClass]
    public class IntegrationSmokeTest : IntegrationTestBase
    {
        [TestMethod, TestCategory("IntegrationSmoke")]
        public void EmployeeEarningsSmoke()
        {
            TryUploadFile("Employee Earning API.xlsx");
        }
    }
}

