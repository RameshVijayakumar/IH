using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paycor.Import.ImportHubTest.Common
{
    class ImportHubTestException : Exception
    {
        public ImportHubTestException() : base()
        {
            
        }

        public ImportHubTestException(string message) : base(message)
        {
            
        }

        public ImportHubTestException(string message, Exception innerException) :
            base(message, innerException)
        {
            
        }
    }
}
