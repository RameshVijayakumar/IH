using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paycor.Import.ImportHubTest.Common
{
    public class ApiVersion
    {
        public string ServiceName { get; private set; }
        public string Version { get; private set; }

        public string ServiceRoute => $"{ServiceName}/{Version}";

        public ApiVersion(string serviceName, string version)
        {
            ServiceName = serviceName;
            Version = version;
        }


    }
}
