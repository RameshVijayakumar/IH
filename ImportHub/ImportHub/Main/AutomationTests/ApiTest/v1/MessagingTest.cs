using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.ImportHubTest.ApiTest.TestBase;
using Paycor.Import.ImportHubTest.ApiTest.Types;

namespace Paycor.Import.ImportHubTest.ApiTest.v1
{
    [TestClass]
    public class MessagingTest : ApiTestBase
    {
        [TestMethod, TestCategory("MessagingTest")]
        public async Task SendMessagingTest()
        {
            var payload =
                "{\r\n  \"id\": \"string\",\r\n  \"initiator\": {\r\n \"id\": \"string\",\r\n \"displayName\": \"string\"\r\n  },\r\n  \"recipients\": [\r\n    {\r\n   \"id\": \"string\",\r\n   \"displayName\": \"string\"\r\n    }\r\n  ],\r\n  \"purpose\": \"string\",\r\n  \"shortMessage\": \"string\",\r\n  \"mediumMessage\": \"string\",\r\n  \"longMessage\": \"string\",\r\n  \"messageTypeId\": \"string\",\r\n  \"metaData\": {}\r\n}";
            var response = await TestClient.MessagePost(payload);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);           
        }

        [TestMethod, TestCategory("MessagingTest")]
        public async Task SendMessagingEmptyPayloadTest()
        {
            var payload = "{}";
            var response = await TestClient.MessagePost(payload);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Detail, "An error has occurred.");
        }

        [TestMethod, TestCategory("MessagingTest")]
        public async Task SendMessagingBatchTest()
        {
            var payload =
                "[\r\n  {\r\n \"id\": \"string\",\r\n \"initiator\": {\r\n   \"id\": \"string\",\r\n   \"displayName\": \"string\"\r\n    },\r\n \"recipients\": [\r\n      {\r\n     \"id\": \"string\",\r\n     \"displayName\": \"string\"\r\n      }\r\n    ],\r\n \"purpose\": \"string\",\r\n \"shortMessage\": \"string\",\r\n \"mediumMessage\": \"string\",\r\n \"longMessage\": \"string\",\r\n \"messageTypeId\": \"string\",\r\n \"metaData\": {}\r\n  }\r\n]";
            var response = await TestClient.MessagePost(payload);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod, TestCategory("MessagingTest")]
        public async Task SendMessagingBatchEmptyPayloadTest()
        {
            var payload = "[]";
            var response = await TestClient.MessagePost(payload);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Detail, "An error has occurred.");
        }

        [TestMethod, TestCategory("MessagingTest")]
        public async Task SendMessagingBatchInvalidBathPayloadTest()
        {
            var payload = "[[\r\n  {\r\n \"id\": \"string\",\r\n \"initiator\": {\r\n   \"id\": \"string\",\r\n   \"displayName\": \"string\"\r\n    }]";
            var response = await TestClient.MessagePost(payload);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Detail, "An error has occurred.");
        }
    }
}
