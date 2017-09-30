using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.ImportHubTest.ApiTest.TestBase;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.v1
{
    /// <summary>
    /// Summary description for FileUploadTest
    /// </summary>
    [TestClass]
    public class FileUploadTest : ApiTestBase
    {
        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\MockV1_Game_empty_Action.csv")]
        //[Bug("309083", "http://cintfs02.cinci.paycor.com:8080/tfs/Paycor/Paycor%20Inc/_workitems/edit/309083", SupressError = true)]
        public async Task UploadFileCompletedTest()
        {
            //get file
            string file = "MockV1_Game_empty_Action.csv";
            string[] mapList = new[] {"ImportHub Mock V1 - Game"};
            Assert.IsTrue(File.Exists(file));
            await TestClient.UploadMappedFile(file, mapList);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\MockV1_Game_no_Action_Column.csv")]
        public async Task UploadFileCompletedNoActionColumnCsvTest()
        {
            //get file
            string file = "MockV1_Game_no_Action_Column.csv";
            string[] mapList = new[] { "ImportHub Mock V1 - Game" };
            Assert.IsTrue(File.Exists(file));
            await TestClient.UploadMappedFile(file, mapList);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\MockV1_Game_no_Action_Column.xlsx")]
        public async Task UploadFileCompletedNoActionColumnXlsxTest()
        {
            //get file
            string file = "MockV1_Game_no_Action_Column.xlsx";
            string[] mapList = new[] { "ImportHub Mock V1 - Game" };
            Assert.IsTrue(File.Exists(file), $"File not found= {file}");
            await TestClient.UploadMappedFile(file, mapList);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\MockV3_Game_Event_Driver.xlsx")]
        public async Task UploadMultipleSheetExcelTest()
        {
            //get file
            string file = "MockV3_Game_Event_Driver.xlsx";
            string[] mapList = new[] { "ImportHub Mock V3 - Game", "ImportHub Mock V3 - Event", "ImportHub Mock V3 - Driver" };
            Assert.IsTrue(File.Exists(file));
            await TestClient.UploadMappedFile(file, mapList);
       }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\Multi-tabs EE Import 116915.xlsx", "UploadSmokeExcelTest")]
        public async Task UploadSmokeExcelTest()
        {
            var file = @"UploadSmokeExcelTest\Multi-tabs EE Import 116915.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);

            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - Venue.xlsx", "MockApi")]
        public async Task ImportHubMockV3Venue()
        {
            var file = @"MockApi\ImportHub Mock V3 - Venue.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - Team.xlsx", "MockApi")]
        public async Task ImportHubMockV3Team()
        {
            var file = @"MockApi\ImportHub Mock V3 - Team.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - Game.xlsx", "MockApi")]
        public async Task ImportHubMockV3Game()
        {
            var file = @"MockApi\ImportHub Mock V3 - Game.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - FeauxEmployee.xlsx", "MockApi")]
        public async Task ImportHubMockV3FeauxEmployee()
        {
            var file = @"MockApi\ImportHub Mock V3 - FeauxEmployee.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - Event.xlsx", "MockApi")]
        public async Task ImportHubMockV3FeauxEvent()
        {
            var file = @"MockApi\ImportHub Mock V3 - Event.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - Driver.xlsx", "MockApi")]
        public async Task ImportHubMockV3Driver()
        {
            var file = @"MockApi\ImportHub Mock V3 - Driver.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V2 - Venue.xlsx", "MockApi")]
        public async Task ImportHubMockV2Venue()
        {
            var file = @"MockApi\ImportHub Mock V2 - Venue.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V2 - Driver.xlsx", "MockApi")]
        public async Task ImportHubMockV2Driver()
        {
            var file = @"MockApi\ImportHub Mock V2 - Driver.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V2 - Event.xlsx", "MockApi")]
        public async Task ImportHubMockV2Event()
        {
            var file = @"MockApi\ImportHub Mock V2 - Event.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - F.xlsx", "MockApi")]
        public async Task ImportHubMockV3F()
        {
            var file = @"MockApi\ImportHub Mock V3 - F.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - F.xlsx", "MockApi")]
        public async Task ImportHubMockV3w2()
        {
            var file = @"MockApi\ImportHub Mock V3 - F.xlsx";
            Assert.IsTrue(File.Exists(file));
            //var maps = Utils.GetExcelSheetNames(file);
            var maps = TestClient.GetMapsForExcel(file);
            //await TestClient.UploadMappedFile(file, maps);
        }

        [TestMethod, TestCategory("History"), Ignore]
        [DeploymentItem(@"v1\TestFiles\ImportHub Mock V3 - Driver.xlsx", "MockApi")]
        public async Task ImportHubMockV3DriverLoad()
        {
            var file = @"MockApi\ImportHub Mock V3 - Driver.xlsx";
            Assert.IsTrue(File.Exists(file));
            var maps = Utils.GetExcelSheetNames(file);
            for (int i = 0; i < 100; i++ )
            await TestClient.UploadMappedFile(file, maps);

            //await Task.Delay(TimeSpan.FromMinutes(10));
            //await GetHistoryforCurrentUserTest();
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\MockV1_Game_empty_Action.csv")]
        public async Task UploadFileConstWithNullSource()
        {
            string file = "MockV1_Game_empty_Action.csv";
            var constWithNullSourceMap = "  [\r\n {\r\n    \"id\": \"1691a0dc-463a-493b-8843-4727c8dc6ac3\",\r\n    \"mappingName\": \"ImportHub Mock V1 - Game\",\r\n    \"mappingEndpoints\": {\r\n      \"post\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games\",\r\n      \"put\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games/{id}\",\r\n      \"delete\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games/{id}\",\r\n      \"patch\": null\r\n    },\r\n    \"mapping\": {\r\n      \"fieldDefinitions\": [\r\n        {\r\n          \"source\": null,\r\n          \"destination\": \"gameId\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"int\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"title\",\r\n          \"destination\": \"title\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": true,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"genre\",\r\n          \"destination\": \"genre\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"publisher\",\r\n          \"destination\": \"publisher\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": true,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"rating\",\r\n          \"destination\": \"rating\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"retailPrice\",\r\n          \"destination\": \"retailPrice\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"double\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"publishDate\",\r\n          \"destination\": \"publishDate\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"DateTime\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"clientId\",\r\n          \"destination\": \"clientId\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"id\",\r\n          \"destination\": \"id\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"action\",\r\n          \"destination\": \"ih:action\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        }\r\n      ]\r\n    },\r\n    \"docUrl\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/swagger/docs/v1\",\r\n    \"user\": null,\r\n    \"isMappingValid\": true,\r\n    \"isBatchSupported\": false,\r\n    \"isBatchChunkingSupported\": false,\r\n    \"preferredBatchChunkSize\": null,\r\n    \"chunkSize\": 0,\r\n    \"objectType\": \"ImportHub Mock V1 - Game\",\r\n    \"hasHeader\": null,\r\n    \"systemType\": \"Paycor.Import.Mapping.ApiMapping\"\r\n  }\r\n  ]";
            Assert.IsTrue(File.Exists(file));
            await TestClient.UploadWithCustomMap(file, constWithNullSourceMap);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\MockV1_Game_empty_Action.csv")]
        public async Task UploadFileConstWithEmptySpaceSource()
        {
            string file = "MockV1_Game_empty_Action.csv";
            var constWithNullSourceMap = "  [\r\n {\r\n    \"id\": \"1691a0dc-463a-493b-8843-4727c8dc6ac3\",\r\n    \"mappingName\": \"ImportHub Mock V1 - Game\",\r\n    \"mappingEndpoints\": {\r\n      \"post\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games\",\r\n      \"put\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games/{id}\",\r\n      \"delete\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games/{id}\",\r\n      \"patch\": null\r\n    },\r\n    \"mapping\": {\r\n      \"fieldDefinitions\": [\r\n        {\r\n          \"source\": \"\",\r\n          \"destination\": \"gameId\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"int\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"title\",\r\n          \"destination\": \"title\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": true,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"genre\",\r\n          \"destination\": \"genre\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"publisher\",\r\n          \"destination\": \"publisher\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": true,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"rating\",\r\n          \"destination\": \"rating\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"retailPrice\",\r\n          \"destination\": \"retailPrice\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"double\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"publishDate\",\r\n          \"destination\": \"publishDate\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"DateTime\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"clientId\",\r\n          \"destination\": \"clientId\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"id\",\r\n          \"destination\": \"id\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"action\",\r\n          \"destination\": \"ih:action\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        }\r\n      ]\r\n    },\r\n    \"docUrl\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/swagger/docs/v1\",\r\n    \"user\": null,\r\n    \"isMappingValid\": true,\r\n    \"isBatchSupported\": false,\r\n    \"isBatchChunkingSupported\": false,\r\n    \"preferredBatchChunkSize\": null,\r\n    \"chunkSize\": 0,\r\n    \"objectType\": \"ImportHub Mock V1 - Game\",\r\n    \"hasHeader\": null,\r\n    \"systemType\": \"Paycor.Import.Mapping.ApiMapping\"\r\n  }\r\n  ]";
            Assert.IsTrue(File.Exists(file));
            await TestClient.UploadWithCustomMap(file, constWithNullSourceMap);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\MockV1_Game_empty_Action.csv")]
        public async Task UploadFileConstWithmOneSpaceSource()
        {
            string file = "MockV1_Game_empty_Action.csv";
            var constWithNullSourceMap = " [ {\r\n    \"id\": \"1691a0dc-463a-493b-8843-4727c8dc6ac3\",\r\n    \"mappingName\": \"ImportHub Mock V1 - Game\",\r\n    \"mappingEndpoints\": {\r\n      \"post\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games\",\r\n      \"put\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games/{id}\",\r\n      \"delete\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games/{id}\",\r\n      \"patch\": null\r\n    },\r\n    \"mapping\": {\r\n      \"fieldDefinitions\": [\r\n        {\r\n          \"source\": \" \",\r\n          \"destination\": \"gameId\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"int\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 1,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"title\",\r\n          \"destination\": \"title\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": true,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"genre\",\r\n          \"destination\": \"genre\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"publisher\",\r\n          \"destination\": \"publisher\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": true,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"rating\",\r\n          \"destination\": \"rating\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"retailPrice\",\r\n          \"destination\": \"retailPrice\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"double\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"publishDate\",\r\n          \"destination\": \"publishDate\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"DateTime\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"clientId\",\r\n          \"destination\": \"clientId\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"id\",\r\n          \"destination\": \"id\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"action\",\r\n          \"destination\": \"ih:action\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        }\r\n      ]\r\n    },\r\n    \"docUrl\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/swagger/docs/v1\",\r\n    \"user\": null,\r\n    \"isMappingValid\": true,\r\n    \"isBatchSupported\": false,\r\n    \"isBatchChunkingSupported\": false,\r\n    \"preferredBatchChunkSize\": null,\r\n    \"chunkSize\": 0,\r\n    \"objectType\": \"ImportHub Mock V1 - Game\",\r\n    \"hasHeader\": null,\r\n    \"systemType\": \"Paycor.Import.Mapping.ApiMapping\"\r\n  }]";
            Assert.IsTrue(File.Exists(file));
            await TestClient.UploadWithCustomMap(file, constWithNullSourceMap);
        }

        [TestMethod, TestCategory("FileUpload")]
        [DeploymentItem(@"v1\TestFiles\MockV1_Game_empty_Action.csv")]
        public async Task UploadFileConstWithmMuiltiSpaceSource()
        {
            string file = "MockV1_Game_empty_Action.csv";
            var constWithNullSourceMap = " [ {\r\n    \"id\": \"1691a0dc-463a-493b-8843-4727c8dc6ac3\",\r\n    \"mappingName\": \"ImportHub Mock V1 - Game\",\r\n    \"mappingEndpoints\": {\r\n      \"post\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games\",\r\n      \"put\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games/{id}\",\r\n      \"delete\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/importhubrefservice/v1/gamecatalog/games/{id}\",\r\n      \"patch\": null\r\n    },\r\n    \"mapping\": {\r\n      \"fieldDefinitions\": [\r\n        {\r\n          \"source\": \"    \",\r\n          \"destination\": \"gameId\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"int\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 1,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"title\",\r\n          \"destination\": \"title\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": true,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"genre\",\r\n          \"destination\": \"genre\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"publisher\",\r\n          \"destination\": \"publisher\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": true,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"rating\",\r\n          \"destination\": \"rating\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"retailPrice\",\r\n          \"destination\": \"retailPrice\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"double\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"publishDate\",\r\n          \"destination\": \"publishDate\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"DateTime\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"clientId\",\r\n          \"destination\": \"clientId\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"id\",\r\n          \"destination\": \"id\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        },\r\n        {\r\n          \"source\": \"action\",\r\n          \"destination\": \"ih:action\",\r\n          \"globalLookupType\": null,\r\n          \"type\": \"string\",\r\n          \"required\": false,\r\n          \"endPoint\": null,\r\n          \"valuePath\": null,\r\n          \"sourceType\": 0,\r\n          \"exceptionMessage\": null,\r\n          \"isRequiredForPayload\": false\r\n        }\r\n      ]\r\n    },\r\n    \"docUrl\": \"http://devsbqtrweb01.dev.paycor.com/importhubrefservice/swagger/docs/v1\",\r\n    \"user\": null,\r\n    \"isMappingValid\": true,\r\n    \"isBatchSupported\": false,\r\n    \"isBatchChunkingSupported\": false,\r\n    \"preferredBatchChunkSize\": null,\r\n    \"chunkSize\": 0,\r\n    \"objectType\": \"ImportHub Mock V1 - Game\",\r\n    \"hasHeader\": null,\r\n    \"systemType\": \"Paycor.Import.Mapping.ApiMapping\"\r\n  }]";
            Assert.IsTrue(File.Exists(file));
            await TestClient.UploadWithCustomMap(file, constWithNullSourceMap);
        }
    }
}
