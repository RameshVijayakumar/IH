using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;
using static Paycor.Import.ImportHubTest.Common.Utils;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    public partial class ApiTestClient
    {

        public async Task<HttpResponseMessage> FileGetStatus(string transactionId, bool getDetails = false)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/files/{transactionId}?statusDetails={getDetails}";
            return await CallGet(query);
        }

        public async Task<HttpResponseMessage> FileGetFailedRows(string id)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/files/{id}/failedrows";
            return await CallGet(query);
        }

        protected async Task<IEnumerable<Map>> GetMapsByName(IEnumerable<string> mapNames, bool registeredMaps = true, bool allowDupe = false)
        {
            List<Map> result = new List<Map>();
            HashSet<string> names = new HashSet<string>(mapNames);
            try
            {
                var response = await MapGetCurrentUser(registeredMaps);
                var maps = response.Content.ReadAsStringAsync().Result.Deserialize<Map>();
                foreach (var map in maps)
                {
                    if (names.Contains(map.MappingName))
                    {
                        result.Add(map);
                        if (!allowDupe)
                            names.Remove(map.MappingName);
                    }
                }
                if (names.Count > 0)
                {
                    throw new AutomationTestException($"Cannot find matched maps with these mappingNames [{string.Join(", ", names)}]");
                }
            }
            catch (Exception ex)
            {
                throw new AutomationTestException("Unable to retrieve maps for current user\n" + ex.StackTrace);
            }
            return result;
        }

        public async Task UploadEmployeeImportFile(string filename)
        {
            var request = new FileUploadRequest(filename, null);
            var query = $"{GetServicePath(ImporthubRoute)}/files?fileTypeMappingInfo.fileType={FileTypeMappingInfo.EmployeeImport}";
            var response = await CallPost(query, request.ToString());
        }

        public async  Task UploadWithCustomMap(string filename, string customMap, bool registered = true)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/files";
            var maps = customMap.Deserialize<Map>();
            var content = GetContent(maps, filename);
            var response = await CallPost(query, content);
            var referenceId = response.Content.ReadAsStringAsync().Result.Trim('"');
            var progStatus = FileGetStatus(referenceId, true).Result;
            Console.WriteLine(referenceId);
            Assert.AreEqual(HttpStatusCode.OK, progStatus.StatusCode);

        }

        public async Task UploadMappedFile(string filename, IEnumerable<string> mapNames, bool registered = true)
        {
            //?fileTypeMappingInfo.fileType={FileTypeMappingInfo.MappedFileImport}
            var maps = await GetMapsByName(mapNames, registered);
            var query = $"{GetServicePath(ImporthubRoute)}/files";
            var content = GetContent(maps, filename);
            var response = await CallPost(query, content);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            //var location = response.Headers.Location.AbsoluteUri;
            //var progress = CallGet(location).Result;
            var referenceId = response.Content.ReadAsStringAsync().Result.Trim('"');
            var progStatus = FileGetStatus(referenceId, true).Result;
            Assert.AreEqual(HttpStatusCode.OK, progStatus.StatusCode);
            //var result = TryParseJson(response.ToString());

        }

        public MultipartFormDataContent GetContent(IEnumerable<Map> maps, string filename)
        {
            var content = new MultipartFormDataContent
            {
                {new StreamContent(new FileStream(filename, FileMode.Open)), "file", Path.GetFileName(filename)},
            };

            if (maps.Count() == 1)
            {
                content.Add(new StringContent(maps.FirstOrDefault().ToString()), "mapping");
            }
            else if (maps.Count() > 1)
            {
                content.Add(new StringContent(maps.Serialize()), "mapping");
            }
            else
            {
                throw new AutomationTestException("No mapping attached!!!");
            }
            return content;
        }

        public IEnumerable<Map> GetMapsForExcel(string filename)
        {
            var dictionary = new Dictionary<string, List<string>>();

            using (var file = new FileStream(filename, FileMode.Open))
            {
                var excel = new ExcelPackage(file);
                foreach (var sheet in excel.Workbook.Worksheets)
                {
                    var name = sheet.Name;
                    var rows = new List<string>();
                    for (var i = 0; i < sheet.Dimension.Rows; i++)
                    {
                        if (i == 3)
                        {
                            break;
                        }
                        rows.Add(sheet.Row(i).ToString());
                    }
                    while (rows.Count < 4)
                    {
                        rows.Add("");
                    }
                    dictionary.Add(name, rows);
                }
            }


            foreach (var name in dictionary.Keys)
            {
                var response = FileTypePost(dictionary[name].ToString()).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var map = content.Deserialize<Map>();
                return map;
            }
            return null;
        }

        public static IEnumerable<string> GetExcelSheetNames(string filename)
        {
            List<string> names = new List<string>();
            try
            {
                using (var file = new FileStream(filename, FileMode.Open))
                {
                    var excel = new ExcelPackage(file);
                    names.AddRange(excel.Workbook.Worksheets.Select(s => s.Name));
                }
            }
            catch (Exception)
            {
                Log($"Error occured when parse file {filename}");
                throw;
            }
            return names;
        }
    }
}
