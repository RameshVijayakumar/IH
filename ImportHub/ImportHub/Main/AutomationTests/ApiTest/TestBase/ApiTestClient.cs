using System.Collections.Generic;
using System.Text.RegularExpressions;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    public partial class ApiTestClient : TestClientBase
    {
        const string ImporthubRoute = "/importhubservice/importhub";
        const string MessagingRoute = "/messagingservice/messaging";
        protected string CorrelationId { get; set; }
        protected string ImportStatus { get; set; }
        protected string TransactionId { get; set; }
        protected string ServiceVersion { get; set; }
        protected string ServiceRoute { get; set; }

        protected IEnumerable<string> GetStatusProgress(string transactionId) { yield return ImportStatus; }

        public ApiTestClient()
        {
            // set default api version to 'v1'
            SetApiVersion("v1");
        }

        protected void SetApiVersion(string version)
        {
            if (!string.IsNullOrEmpty(version) && Regex.IsMatch(version, "v[0-9]+", RegexOptions.IgnoreCase))
            {
                ServiceVersion = version;
            }
            else
            {
                throw new AutomationTestException($"Unknow versioning {version}");
            }
        }

        private string GetServicePath(string serviceRoute)
        {
            return $"{serviceRoute}/{ServiceVersion}";
        }
    }
}