using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.ImportHubTest.ApiTest.TestBase;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;
using static Paycor.Import.ImportHubTest.Common.Utils;

namespace Paycor.Import.ImportHubTest.ApiTest.v1
{
    [TestClass]
    public class MappingTests : ApiTestBase
    {
        [TestMethod, TestCategory("Mappings"), TestCategory("ContinuesIntegration")]
        public async Task GetAllRegisteredMappingsForCurrentUserTest()
        {
            var response = await TestClient.MapGetCurrentUser(true);
            var content = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotNull(content);
            Assert.IsTrue(content.Deserialize<Map>().Any());
            Log(content.Deserialize<Map>().Count());
            Log(content);
        }

        [TestMethod, TestCategory("Mappings"), TestCategory("ContinuesIntegration")]
        public async Task GetNonRegisteredMappingsForCurrentUserTest()
        {
            var response = await TestClient.MapGetCurrentUser(false);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "NotFound");
            Assert.IsNotNull(msg.CorrelationId);
        }

        [TestMethod, TestCategory("Mappings"), TestCategory("ContinuesIntegration")]
        public async Task GetAlldMappingsWithoutParameterTest()
        {
            var response = await TestClient.MapGetCurrentUser(null);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "NotFound");
            Assert.IsNotNull(msg.CorrelationId);
        }

        [TestMethod, TestCategory("Mappings"), TestCategory("ContinuesIntegration")]
        public async Task GetMappingByValidIdTest()
        {
            var response = await TestClient.MapGetCurrentUser(true);
            var map = response.Content.ReadAsStringAsync().Result.Deserialize<Map>().FirstOrDefault();
            Assert.IsNotNull(map);
            var id = map.Id;
            response = await TestClient.MapGetById(map.Id);
            map = JsonConvert.DeserializeObject<Map>(response.Content.ReadAsStringAsync().Result);
            Log(map);
            Assert.IsNotNull(map.Id);
            Assert.IsTrue(map.Mapping.FieldDefinitions.ToList<MappingField>().Count > 0);
        }

        [TestMethod, TestCategory("Mappings"), TestCategory("ContinuesIntegration")]
        public async Task GetMappingByInValidIdTest()
        {
            var id = "xx165a602-9c22-450c-b0f5-c3c41e12d314";
            var response = await TestClient.MapGetById(id);
            Assert.IsFalse(response.IsSuccessStatusCode);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "NotFound");
            Assert.IsNotNull(msg.CorrelationId);
            Assert.AreEqual(msg.Title, "Item not found");
            Assert.AreEqual(msg.Detail, "The requested item could not be found.");
            Assert.AreEqual(msg.Source["id"], "could not be found.");
        }

        [TestMethod, TestCategory("Mappings"), TestCategory("ContinuesIntegration")]
        public async Task GetMappingByEmptyIdTest()
        {
            var id = " ";
            var response = await TestClient.MapGetById(id);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "NotFound");
            Assert.IsNotNull(msg.CorrelationId);
        }

        [TestMethod, TestCategory("Mappings"), TestCategory("ContinuesIntegration")]
        public async Task GetMappingBySpecialCharIdTest()
        {
            string[] ids = { "$", "/", "\\", "?"};
            foreach (var id in ids)
            {
                var response = await TestClient.MapGetById(id);
                var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
                Assert.AreEqual(msg.Status, "NotFound");
                Assert.IsNotNull(msg.Title);
                Assert.IsNotNull(msg.CorrelationId);
                Assert.IsNotNull(msg.Detail);
            }
        }
    }
}
