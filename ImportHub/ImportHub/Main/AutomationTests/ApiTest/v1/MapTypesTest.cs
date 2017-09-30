using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.ImportHubTest.ApiTest.TestBase;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.v1
{
    [TestClass]
    public class MapTypesTest : ApiTestBase
    {
        [TestMethod, TestCategory("MapTypes"), TestCategory("ContinuesIntegration")]
        public async Task GetMapTypesInvalidPayload()
        {
            string payload = "{\r\n  \"objectType\": \"string\",\r\n  \"payload\": [\r\n    \"single Column\"\r\n  ]\r\n}";
            var response = await TestClient.MapTypePost(payload);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "BadRequest");
            Assert.IsNotNull(msg.CorrelationId);
            Assert.AreEqual(msg.Title, "Validation Error");
            Assert.IsTrue(msg.Detail.Contains("validation error occurred"));
            Assert.IsTrue(msg.Source["not Supported"].Contains("Operation not supported"));
        }

        [TestMethod, TestCategory("MapTypes"), TestCategory("ContinuesIntegration")]
        public async Task GetMapTypesMissingFileTypeDefinition()
        {
            string payload = "{\r\n  \"payload\": [\r\n    \"single Column\"\r\n  ]\r\n}";
            var response = await TestClient.MapTypePost(payload);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "BadRequest");
            Assert.IsNotNull(msg.CorrelationId);
            Assert.AreEqual(msg.Title, "Validation Error");
            Assert.AreEqual(msg.Detail, "A validation error occurred. See the source for more information about specific errors.");
            Assert.AreEqual(msg.Source["not Supported"], "Operation not supported on non mapped file types. File type detected as Unrecognized.");
        }

        [TestMethod, TestCategory("MapTypes"), TestCategory("ContinuesIntegration")]
        public async Task GetMapTypesEmptyPayload()
        {
            string payload = "{}";
            var response = await TestClient.MapTypePost(payload);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "InternalServerError");
            Assert.IsNotNull(msg.CorrelationId);
            Assert.AreEqual(msg.Title, "Error Message Title");
            Assert.AreEqual(msg.Detail, "Value cannot be null.\r\nParameter name: source");
        }

        [TestMethod, TestCategory("MapTypes"), TestCategory("ContinuesIntegration")]
        public async Task GetFileTypesSingColumnMatchedTest()
        {
            var payload = @"[[',']]";
            var response = await TestClient.FileTypePost(payload);
            var content = JsonConvert.DeserializeObject<ImportFileTypes>(response.Content.ReadAsStringAsync().Result);
            Assert.IsNull(content.ColumnHeaders);
            Assert.AreEqual(content.ColumnCount, 2);
            Assert.IsTrue(content.AllMappings.Count() > 1);
            Assert.IsTrue(content.Mappings.Count() > 1);
            Assert.AreEqual(content.FileType, "MappedFileImport");
        }

        [TestMethod, TestCategory("MapTypes"), TestCategory("ContinuesIntegration")]
        public async Task GetFileTypesExactMatched_HeaderOnly_Test()
        {
            var payload = "[[\"gameId,title,genre,publisher,rating,retailPrice,publishDate,clientId,id,action\"],[\"\"],[\"\"],[\"\"]]";
            var response = await TestClient.FileTypePost(payload);
            var content = JsonConvert.DeserializeObject<ImportFileTypes>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(content.Mappings.Count(), 1);
            Assert.AreEqual(content.Mappings.FirstOrDefault().MappingName, "ImportHub Mock V1 - Game");
        }

        [TestMethod, TestCategory("MapTypes"), TestCategory("ContinuesIntegration")]
        [Bug("309092", "http://cintfs02.cinci.paycor.com:8080/tfs/Paycor/Paycor%20Inc/_workitems/edit/309092")]
        public async Task GetFileTypesExactMatched_StringArrayType_Test()
        {
            var data = new string[]
            {
                "gameId,title,genre,publisher,rating,retailPrice,publishDate,clientId,id,action" ,
                "123,title_1,genere_1,pblsh_1,23,0.05,06/06/2016,36635",
                "",
                ""
            };

            var payload = JsonConvert.SerializeObject(data);
            var response = await TestClient.FileTypePost(payload);
            var content = JsonConvert.DeserializeObject<ImportFileTypes>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(content.Mappings.Count(), 1);
            Assert.AreEqual(content.Mappings.FirstOrDefault().MappingName, "ImportHub Mock V1 - Game");
        }

        [TestMethod, TestCategory("MapTypes"), TestCategory("ContinuesIntegration")]
        public async Task GetFileTypesExactMatched_ArrayType_Test()
        {
            var data = new string[][]
            {
                new string[]{"gameId,title,genre,publisher,rating,retailPrice,publishDate,clientId,id,action" },
                new string[]{"123,title_1,genere_1,pblsh_1,23,0.05,06/06/2016,36635"},
                new string[]{""},
                new string[]{""}
            };

            var payload = JsonConvert.SerializeObject(data);
            var response = await TestClient.FileTypePost(payload);
            var content = JsonConvert.DeserializeObject<ImportFileTypes>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(content.Mappings.Count(), 1);
            Assert.AreEqual(content.Mappings.FirstOrDefault().MappingName, "ImportHub Mock V1 - Game");
        }

        [TestMethod, TestCategory("MapTypes"), TestCategory("ContinuesIntegration")]
        public async Task GetFileTypesExactMatched_ArrayOfArray_Test()
        {

            var data = new List<List<string>>
            {
                new List<string> { "gameId,title,genre,publisher,rating,retailPrice,publishDate,clientId,id,action"}
            };
            data.Add(new List<string> { "123,title_1,genere_1,pblsh_1,23,0.05,06/06/2016,36635," });
            data.Add(new List<string>() { "" });
            data.Add(new List<string>() { "" });

            var payload = JsonConvert.SerializeObject(data);
            var response = await TestClient.FileTypePost(payload);
            var content = JsonConvert.DeserializeObject<ImportFileTypes>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(content.Mappings.Count(), 1);
            Assert.AreEqual(content.Mappings.FirstOrDefault().MappingName, "ImportHub Mock V1 - Game");
        }
    }
}
