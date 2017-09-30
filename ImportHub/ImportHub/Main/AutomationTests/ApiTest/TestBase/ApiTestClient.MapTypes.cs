using System.Net.Http;
using System.Threading.Tasks;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    public partial class ApiTestClient
    {
        public async Task<HttpResponseMessage> MapTypePost(string payload)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/maptypes";
            return await CallPost(query, payload);
        }

        public async Task<HttpResponseMessage> MapGetCurrentUser(bool? isRegisteredMap)
        {
            var parameter = isRegisteredMap == null ? string.Empty : $"?registeredMaps={isRegisteredMap}";
            var query = $"{GetServicePath(ImporthubRoute)}/mappings{parameter}";
            return await CallGet(query);
        }

        public async Task<HttpResponseMessage> MapGetById(string mapId)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/mappings/{mapId}";
            return await CallGet(query);
        }

        public async Task<HttpResponseMessage> MapPost(string payload)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/mappings";
            return await CallPost(query, payload);
        }

        public async Task<HttpResponseMessage> MapDelete(string mapId)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/mappings/{mapId}";
            return await CallDelete(query);
        }

        public async Task<HttpResponseMessage> MapPut(string mapId, string payload)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/mappings/{mapId}";
            return await CallPut(query, payload);
        }

        public async Task<HttpResponseMessage> MapGetTemplate(string mapId)
        {
            var query = $"{GetServicePath(ImporthubRoute)}/mappings/{mapId}/template";
            return await CallGet(query);
        }
    }
}
