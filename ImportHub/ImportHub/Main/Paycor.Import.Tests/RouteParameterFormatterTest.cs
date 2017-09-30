using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RouteParameterFormatterTest
    {
        [TestMethod]
        public void FormatEndPointWith_ValidRouteParamValue()
        {
            var routeParameterFormatter = new RouteParameterFormatter();

            const string endpoint = @"http://localhost:8083/import/referenceapi/v1/gamecatalog/{gameid}/games";
            var record = new Dictionary<string,string>()
            {
                {"gameid","5"}
            };
            Assert.AreEqual("http://localhost:8083/import/referenceapi/v1/gamecatalog/5/games", routeParameterFormatter.FormatEndPointWithParamValue(endpoint, record));
        }


        [TestMethod]
        public void FormatMultipleEndPointWith_ValidRouteParamValue()
        {
            var routeParameterFormatter = new RouteParameterFormatter();

            const string endpoint = @"http://localhost:8083/import/referenceapi/v1/gamecatalog/{gameid}/games/{clientid}";
            var record = new Dictionary<string, string>()
            {
                {"gameid","5"},
                {"clientid","6"}
            };
            Assert.AreEqual("http://localhost:8083/import/referenceapi/v1/gamecatalog/5/games/6",
                routeParameterFormatter.FormatEndPointWithParamValue(endpoint, record));
        }

        [TestMethod]
        public void FormatMultipleEndPointWith_RouteParamValue()
        {
            var routeParameterFormatter = new RouteParameterFormatter();

            const string endpoint = @"http://localhost:8083/import/referenceapi/v1/gamecatalog/{gameid}/games/{clientid}";
            var record = new Dictionary<string, string>()
            {
                {"gameid","5"},
                {"clientid","6"},
                {"data","12121"}
            };
            Assert.AreEqual("http://localhost:8083/import/referenceapi/v1/gamecatalog/5/games/6",
                routeParameterFormatter.FormatEndPointWithParamValue(endpoint, record));
        }

        [TestMethod]
        public void FormatEndPointWith_ParamValue()
        {
            var routeParameterFormatter = new RouteParameterFormatter();

            const string endpoint = @"http://localhost:8083/import/referenceapi/v1/gamecatalog/{gameid}/games";
            var record = new Dictionary<string, string>()
            {
                {"gameid","15"}
            };
            Assert.AreEqual("http://localhost:8083/import/referenceapi/v1/gamecatalog/15/games", routeParameterFormatter.FormatEndPointWithParamValue(endpoint, record));
        }

        [TestMethod]
        public void FormatEndPointWith_DateParamValue()
        {
            var routeParameterFormatter = new RouteParameterFormatter();

            const string endpoint = @"http://localhost/employeeservice/v1/employees/{employeeId}/positionhistoryeffectivedates/{effectiveDate}";
            var record = new Dictionary<string, string>()
            {
                {"effectiveDate","01/01/2017"},
                {"employeeId","12325-4587-789654231"}
            };
            Assert.AreEqual("http://localhost/employeeservice/v1/employees/12325-4587-789654231/positionhistoryeffectivedates/01-01-2017", routeParameterFormatter.FormatEndPointWithParamValue(endpoint, record));
        }

        [TestMethod]
        public void RemoveLookupParameters()
        {
            var routeParameterFormatter = new RouteParameterFormatter();

            var record = new Dictionary<string,string>()
            {
                {"{gameid}","5"},
                  {"{clientid}","5"},
                {"game","nfl"}
            };

            var filteredRecord =  routeParameterFormatter.RemoveLookupParameters(record);

            Assert.AreEqual(1,filteredRecord.Count);
        }

        [TestMethod]
        public void HasLookupParameters()
        {
            var routeParameterFormatter = new RouteParameterFormatter();

            var record = new Dictionary<string, string>()
            {
                {"{gameid}","5"},
                {"game","nfl"}
            };

            Assert.IsTrue(routeParameterFormatter.HasLookupParameters(record));
        }

        [TestMethod]
        public void HasNoLookupParameters()
        {
            var routeParameterFormatter = new RouteParameterFormatter();

            var record = new Dictionary<string, string>()
            {
                {"gameid","5"},
                {"game","nfl"}
            };

            Assert.IsFalse(routeParameterFormatter.HasLookupParameters(record));
        }
    }
}
