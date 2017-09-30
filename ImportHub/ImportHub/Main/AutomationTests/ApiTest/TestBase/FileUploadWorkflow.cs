using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{
    public class FileUploadWorkflow : TestClientBase
    {

        ResourceManager resource = new ResourceManager("AutomationTestError", Assembly.GetExecutingAssembly());
        StringBuilder _logTrace = new StringBuilder();
        private static readonly ILog logger = LogManager.GetLogger((typeof(FileUploadWorkflow)));
        public String File { get; private set; }

        public void LoadFile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                
            }
            string[] buffer = new string[4];
        }

        public string GetMatchMapTyps(string[] sample)
        {
            return "id";
        }

        public string GeneratePayload(string mapId, string filename)
        {
            return "payload";
        }

        public async Task<HttpResponseMessage> Upload()
        {
            throw new NotImplementedException();
        }



        public void GetUploadStatus(String uri)
        {
        }
    }
}
