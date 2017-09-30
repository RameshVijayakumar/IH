using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.IntegrationSmokeTest.TestBase
{
    public partial class IntegrationTestClient : TestClientBase
    {
        const string ImporthubRoute = @"/importhubservice/importhub/v1";
        protected string CorrelationId { get; set; }
        protected string ImportStatus { get; set; }
        protected string TransactionId { get; set; }
        protected string ServiceVersion { get; set; }
        protected string ServiceRoute { get; set; }

        protected IEnumerable<string> GetStatusProgress(string transactionId) { yield return ImportStatus; }

        public IntegrationTestClient()
        {
        }

 
        private string GetServicePath(string serviceRoute)
        {
            return $"{serviceRoute}/{ServiceVersion}";
        }

        public void UploadFileUsingCustomMap(string file, string[] maps)
        {
            
        }

        public void UploadFileUsingRegisteredMap(string file, string[] mapNames)
        {
            
        }

        public void UploadMappedFile(string filename, IEnumerable<string> mapNames, bool registered = true)
        {
            ////?fileTypeMappingInfo.fileType={FileTypeMappingInfo.MappedFileImport}
            //var maps = await GetMapsByName(mapNames, registered);
            //var query = $"{GetServicePath(ImporthubRoute)}/files";
            //var content = GetContent(maps, filename);
            //var response = await CallPost(query, content);
            //Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            ////var location = response.Headers.Location.AbsoluteUri;
            ////var progress = CallGet(location).Result;
            //var referenceId = response.Content.ReadAsStringAsync().Result.Trim('"');
            //var progStatus = FileGetStatus(referenceId, true).Result;
            //Assert.AreEqual(HttpStatusCode.OK, progStatus.StatusCode);
            ////var result = TryParseJson(response.ToString());

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
                //var response = FileTypePost(dictionary[name].ToString()).Result;
                //var content = response.Content.ReadAsStringAsync().Result;
                //var map = content.Deserialize<Map>();
                //return map;
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
                //Log($"Error occured when parse file {filename}");
                throw;
            }
            return names;
        }
    }
}