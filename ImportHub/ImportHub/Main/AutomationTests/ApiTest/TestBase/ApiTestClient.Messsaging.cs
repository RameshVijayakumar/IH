using System.Net.Http;
using System.Threading.Tasks;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    public partial class ApiTestClient
    {
        public async Task<HttpResponseMessage> MessageTypRegistrationGetAll()
        {
            var query = $"{GetServicePath(MessagingRoute)}/messagetyperegistrations";
            return await CallGet(query);
        }

        public async Task<HttpResponseMessage> MessageTypRegistrationPost(string payload)
        {
            var query = $"{GetServicePath(MessagingRoute)}/messagetyperegistrations";
            return await CallPost(query, payload);
        }

        public async Task<HttpResponseMessage> MessageTypRegistrationGetById(string messageId)
        {
            var query = $"{GetServicePath(MessagingRoute)}/messagetyperegistrations/{messageId}";
            return await CallGet(query);
        }

        public async Task<HttpResponseMessage> MessageTypRegistrationDelete(string messageId)
        {
            var query = $"{GetServicePath(MessagingRoute)}/messagetyperegistrations/{messageId}";
            return await CallDelete(query);
        }

        public async Task<HttpResponseMessage> MessageTypRegistrationPut(string messageId, string payload)
        {
            var query = $"{GetServicePath(MessagingRoute)}/messagetyperegistrations/{messageId}";
            return await CallPut(query, payload);
        }

        public async Task<HttpResponseMessage> MessageTypRegistrationTest()
        {
            var query = $"{GetServicePath(MessagingRoute)}/test";
            return await CallGet(query);
        }

        public async Task<HttpResponseMessage> MessagePost(string payload)
        {
            var query = $"{GetServicePath(MessagingRoute)}/messages";
            return await CallPost(query, payload);
        }

        public async Task<HttpResponseMessage> MessageBatchPost(string payload)
        {
            var query = $"{GetServicePath(MessagingRoute)}/messages/batch";
            return await CallPost(query, payload);
        }
    }
}
