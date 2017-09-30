using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.ImportHubTest.ApiTest.TestBase;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;
using static System.IO.File;
using static Paycor.Import.ImportHubTest.Common.Utils;


namespace Paycor.Import.ImportHubTest.ApiTest.v1
{
    [TestClass]
    [DeploymentItem("TestData\\SystemCheckData.txt")]
   // [Bug("000000", "Need pccin account keys to run")]
    public class UnversionedApiTest : ApiTestBase
    {
        [TestMethod, TestCategory("SystemCheck")]
        public async Task HealthCheck()
        {

            var response = await TestClient.HealthCheck();
            var content = response.Content.ReadAsStringAsync().Result;
            Dictionary<string, string> failedTopics = new Dictionary<string, string>();
            //await Task.Delay(1);
            //string content = Exists("SystemCheckData.txt") ? ReadAllText("SystemCheckData.txt") : string.Empty;
            var topics = content.Deserialize<SystemCheckResponse>();
            foreach (var t in topics)
            {
                if(t.Result != "1")
                    Log($"Fail: {t.Name}, {t.Result}");
                    failedTopics.Add(t.Name, t.Result);
            }
            Assert.IsTrue(failedTopics.Count ==0 );
            //wic.Undo();
        }

        [TestMethod, TestCategory("SystemCheck")]
        public async Task MappingDuplicationSmoke()
        {
            bool passed = true;
            string content = Exists("SystemCheckData.txt") ? ReadAllText("SystemCheckData.txt") : string.Empty;
            var topics = content.Deserialize<SystemCheckResponse>();
            Dictionary<string, List<string>> mapDictionary = new Dictionary<string, List<string>>();
            foreach (var t in topics)
            {
                if (t.Name == "Mappings DocumentDb")
                {
                    var maps = t.AdditionalInformation.Deserialize<Map>();
                    foreach (var m in maps)
                    {
                        Assert.IsNotNull(m);
                        string key = $"name= {m.MappingName},    docUrl= ({m.DocUrl})";
                        if (mapDictionary.ContainsKey(key))
                            mapDictionary[key].Add(m.Id);
                        else
                            mapDictionary.Add(key, new List<string>() {m.Id});
                    }
                }
            }
            foreach (var entry in mapDictionary)
            {
                if (entry.Value.Count > 1)
                {
                    passed = false;
                    Log($"Duplicate Map: {entry.Value.Count}x  {entry.Key},    ID(s)= {string.Join(" | ", entry.Value)}");
                }
            }
            Assert.IsTrue(passed);
        }

        [TestMethod, TestCategory("SystemCheck")]
        public async Task MappingRegisteredAndCustomizedSmoke()
        {
            var passed = true;
            var content = Exists("SystemCheckData.txt") ? ReadAllText("SystemCheckData.txt") : string.Empty;
            var topics = content.Deserialize<SystemCheckResponse>();
            var mapDictionary = new Dictionary<string, List<string>>();
            foreach (var t in topics)
            {
                if (t.Name == "Mappings DocumentDb")
                {
                    var mapCollection = t.AdditionalInformation;
                    var maps = mapCollection.Deserialize<Map>();
                    foreach (var m in maps)
                    {
                        DoSth(m, mapDictionary);
                    }
                }
            }
            foreach (var entry in mapDictionary)
            {
                if (entry.Value.Count > 1)
                {
                    passed = false;
                    Log($"Duplicate Map: {entry.Value.Count}x  {entry.Key},    ID(s)= {string.Join(" | ", entry.Value)}");
                }
            }
            Assert.IsTrue(passed);
        }

        [TestMethod, TestCategory("SystemCheck")]
        public async Task MappingSmoke()
        {

            var content = Exists("SystemCheckData.txt") ? ReadAllText("SystemCheckData.txt") : string.Empty;
            var topics = content.Deserialize<SystemCheckResponse>().FirstOrDefault(t => t.Name == "Mappings DocumentDb");
            var maps = topics?.AdditionalInformation.Deserialize<Map>();
            var dupMaps = maps.GroupBy(m => m.MappingName).Where(t=>t.Count() > 1).ToList();


            var dups = dupMaps.Aggregate("Duplicate: \n", (x, y) =>
            {
                
                Log($"{x} --> {y}");
                return y.Count() + "x " + y.Key;
            })
            ;
            Assert.IsTrue(maps.Select(m => m.DocUrl != null).Any(), "No registered maps found");
            Assert.AreEqual(dupMaps.Count, 0);
        }

        private void DoSth(Map m, Dictionary<string, List<string>> mapDictionary)
        {
            Assert.IsNotNull(m);
            string key = $"name= {m.MappingName},    docUrl= ({m.DocUrl})";
            if (mapDictionary.ContainsKey(key))
                mapDictionary[key].Add(m.Id);
            else
                mapDictionary.Add(key, new List<string>() { m.Id });
        }
    }
}
