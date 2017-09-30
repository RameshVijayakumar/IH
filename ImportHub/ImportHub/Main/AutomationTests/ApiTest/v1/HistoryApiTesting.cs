using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.ImportHubTest.ApiTest.TestBase;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.v1
{
    [TestClass]
    public class HistoryApiTesting : ApiTestBase
    {
        [TestMethod, TestCategory("History"), TestCategory("ContinuesIntegration")]
        public async Task GetHistoryforCurrentUserTest()
        {
            var response = await TestClient.HistoryGet();
            var content = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotNull(content);
            var histories = content.Deserialize<HistoryResponse>();
            foreach (var h in histories)
            {
                Assert.IsNotNull(h.Id);
            }
        }

        [TestMethod, TestCategory("History"), TestCategory("ContinuesIntegration")]
        public async Task DeleteHistoryforCurrentUserTest()
        {
            var response = await TestClient.HistoryDelete();
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            response = await TestClient.HistoryGet();
            var content = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(content, "[]");
        }

        [TestMethod, TestCategory("History"), TestCategory("ContinuesIntegration")]
        public async Task GetOrDeleteaNonExistentHistoriesforCurrentUserTest()
        {
            var response = await TestClient.HistoryDelete();
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            response = await TestClient.HistoryGet();
            var content = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(content, "[]");

            response = await TestClient.HistoryDelete();
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod, TestCategory("History"), TestCategory("ContinuesIntegration")]
        public async Task DeleteaHistoryInvalidId()
        {
            var response = await TestClient.HistoryDelete("aacc-404f-9488-a87a985fa142");
            Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);
        }

        [TestMethod, TestCategory("History"), Ignore ]
        public async Task DeleteaHistoryById()
        {
            string id = "";
            var response = await TestClient.HistoryDelete(id);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod, TestCategory("History"), TestCategory("ContinuesIntegration")]
        public async Task DeleteaHistoryByIdForOtherUser()
        {
            string id = "d70ce573-86d9-42e9-ac44-6885043ccabc";
            var response = await TestClient.HistoryDelete(id);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);


        }

    }
}
