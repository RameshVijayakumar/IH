using System.Net.Http;
using System.Threading.Tasks;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    public partial class ApiTestClient
    {
        public async Task<HttpResponseMessage> HistoryGet()
        {
            var query = $"{GetServicePath(ImporthubRoute)}/history";
            return await CallGet(query);
        }

        public async Task<HttpResponseMessage> HistoryDelete(string id = null)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/history/{id}";
            return await CallDelete(query);
        }
    }
}
