using System;
using static Paycor.Import.ImportHubTest.Common.Utils;

namespace Paycor.Import.ImportHubTest.Common
{
    [Serializable]
    public class AutomationTestException : Exception
    {
        public AutomationTestException() : base() { }
        public AutomationTestException(string message) : base(message)
        {
            Log($"***Fail: {message}");
        }

        public AutomationTestException(string message, Exception innerException) :
            base(message, innerException)
        {   

        }
    }
}
