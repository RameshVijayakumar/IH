using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.ImportHubTest.ApiTest.TestBase;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;
using static Paycor.Import.ImportHubTest.Common.Utils;

namespace Paycor.Import.ImportHubTest.ApiTest.v1
{
    [TestClass]
    public class AnalyticsTest : ApiTestBase
    {
        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsValidStartEndDateTest()
        {
            var start = "2016-09-05";
            var end = "2016-09-10";
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            ValidateAttachment(response.Content.ReadAsStreamAsync().Result, start, end);
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsValidDateFormatTest()
        {
            var start = "2016/09/01";
            var end = "2016/09/10";
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            ValidateAttachment(response.Content.ReadAsStreamAsync().Result, start, end);
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsRangeAcrossMonthTest()
        {
            var end = DateTime.Now.ToString();
            var start = DateTime.Now.AddMonths(-2).ToString();
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            ValidateAttachment(response.Content.ReadAsStreamAsync().Result, start, end);
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsRangeAcrossYearTest()
        {
            var end = DateTime.Now.ToString();
            var start = DateTime.Now.AddYears(-1).ToString();
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            ValidateAttachment(response.Content.ReadAsStreamAsync().Result, start, end);
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsRangeAcrossTwoDaysTest()
        {
            var weekDay = DateTime.Now;
            if (weekDay.DayOfWeek == DayOfWeek.Sunday || weekDay.DayOfWeek == DayOfWeek.Saturday)
                weekDay = weekDay.AddDays(-3);
            var start = weekDay.ToString();
            var end = weekDay.AddHours(24).ToString();
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            ValidateAttachment(response.Content.ReadAsStreamAsync().Result, start, end);
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsRangeForOneDaysTest()
        {
            var weekDay = DateTime.Now;
            if (weekDay.DayOfWeek == DayOfWeek.Sunday || weekDay.DayOfWeek == DayOfWeek.Saturday)
            {
                weekDay = weekDay.AddDays(-3).Date;
            }
            else
            {
                weekDay = weekDay.Date;
            }
            var start = weekDay.AddHours(1).ToString();
            var end = weekDay.AddHours(23).ToString();
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            ValidateAttachment(response.Content.ReadAsStreamAsync().Result, start, end);
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsInvertStartEndDateTest()
        {
            var end = "2016-09-05";
            var start = "2016-09-10";
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "Error");
            Assert.IsNotNull(msg.CorrelationId);
            Assert.AreEqual(msg.Title, "An error occurred processing your request...");
            Assert.AreEqual(msg.Detail, "The specified date range of 9/10/2016 12:00:00 AM to 9/5/2016 12:00:00 AM is invalid.");
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsInValidStartDateTest()
        {
            var start = "0000-00-00";
            var end = "2016-09-10";
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "Error");
            Assert.IsNotNull(msg.CorrelationId);
            Assert.AreEqual(msg.Title, "An error occurred processing your request...");
            Assert.AreEqual(msg.Detail, "The start date must be specified.");
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsInValidEndDateTest()
        {
            var end = "0000-00-00";
            var start = "2016-09-10";
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "Error");
            Assert.IsNotNull(msg.CorrelationId);
            Assert.AreEqual(msg.Title, "An error occurred processing your request...");
            Assert.AreEqual(msg.Detail, "The end date must be specified.");
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        public async Task GetAnalyticsInValidDateStringTest()
        {
            var end = "2016";
            var start = "2016-09-10";
            var response = await TestClient.AnalyticsGet(start, end);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(msg.Status, "Error");
            Assert.IsNotNull(msg.CorrelationId);
            Assert.AreEqual(msg.Title, "An error occurred processing your request...");
            Assert.AreEqual(msg.Detail, "The end date must be specified.");
        }

        [TestMethod, TestCategory("Analytics"), TestCategory("ContinuesIntegration")]
        [Bug("309112", "http://cintfs02.cinci.paycor.com:8080/tfs/Paycor/Paycor%20Inc/_workitems/edit/309112")]
        public async Task GetAnalyticsInValidDateMonthOnlyTest()
        {
            var start = "2016/09";
            var end = "2016/09";
            var response = await TestClient.AnalyticsGet(start, end);
            //Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
            var msg = JsonConvert.DeserializeObject<ErrorResponse>(response.Content.ReadAsStringAsync().Result);
            //Assert.AreEqual(msg.Status, "Error");
            //Assert.IsNotNull(msg.CorrelationId);
            //Assert.AreEqual(msg.Title, "An error occurred processing your request...");
            //Assert.AreEqual(msg.Detail, "The specified date range of 9/2016 12:00:00 AM to 9/2016 12:00:00 AM is invalid.");
        }
        void ValidateAttachment(Stream stream, string start, string end)
        {
            Assert.IsNotNull(stream);
            DateTime expectedStart;
            DateTime expectedEnd;
            DateTime.TryParse(start, out expectedStart);
            DateTime.TryParse(end, out expectedEnd);

            string filename = $"DownloadAnalytics_{expectedStart.ToString("yyyy-MM-dd-H-mm-ss")}-{expectedEnd.ToString("yyyy-MM-dd-H-mm-ss")}.csv";
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                stream.CopyTo(fs);
                Log($"Download analytics to {filename}");
            } 
            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                int lineCounts = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineCounts++;
                    if (lineCounts == 1)
                    {
                        Assert.AreEqual(line,
                            "StartTime,EndTime,ElapsedTime,TotalRecords,SuccessRecords,ErrorRecords,OriginatingUser,ImportType");
                    }
                    else
                    {
                        var col = line.Split(',');
                        DateTime startTime;
                        DateTime endTime;
                        DateTime.TryParse(col[0], out startTime);
                        DateTime.TryParse(col[1], out endTime);
                        Assert.IsTrue(startTime >= expectedStart);
                        Assert.IsTrue(endTime <= expectedEnd);
                        for (var i = 2; i < col.Length; i++)
                            // Assert.IsFalse(string.IsNullOrEmpty(col[i]), line);  // will fail if there are queued imports
                            Assert.IsNotNull(col[i], line);
                        Assert.AreEqual(Convert.ToUInt64(col[3]), Convert.ToUInt64(col[4]) + Convert.ToUInt64(col[5]), "Calculate record count filed:  Total != success + error");
                    }
                }
                Assert.IsTrue(lineCounts > 0);
            }
        }

    }
}
