using System.Net.Http;
using System.Threading.Tasks;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    public partial class ApiTestClient
    {
        public async Task<HttpResponseMessage> AnalyticsGet(string startDate, string endDate)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/analytics?start={startDate}&end={endDate}";
            return await CallGet(query, Constants.MediaTypeStream);
        }
    }
}
