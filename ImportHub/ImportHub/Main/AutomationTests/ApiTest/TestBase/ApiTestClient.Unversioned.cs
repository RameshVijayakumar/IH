using System.Net.Http;
using System.Threading.Tasks;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    public partial class ApiTestClient
    {
        public async Task<HttpResponseMessage> FileTypePost(string payload)
        {
            var query = "/importhubservice/importhub/filetypes";
            return await CallPost(query, payload);
        }

        public async Task<HttpResponseMessage> RegisteredMapsGet()
        {
            var query = "/importhubservice/importhub/registeredmaps";
            return await CallGet(query);
        }

        public async Task<HttpResponseMessage> RegisteredMapsPose(string payload)
        {
            var query = "/importhubservice/importhub/registeredmaps";
            return await CallPost(query, payload);
        }

        public async Task<HttpResponseMessage> HealthCheck()
        {
            var query = "/importhubservice/api/SystemCheck/HealthCheck";
            return await CallGet(query);
        }
    }
}
