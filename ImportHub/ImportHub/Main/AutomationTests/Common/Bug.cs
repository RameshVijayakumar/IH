using System;
using static Paycor.Import.ImportHubTest.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Paycor.Import.ImportHubTest.Common
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true,  Inherited = true)]
    public sealed class Bug : ExpectedExceptionBaseAttribute
    {
        private readonly string _number;
        private readonly string _link;
        public bool SupressError;
                
        public Bug(string number, string link)
        {
            _number = number;
            _link = link;
        }

        protected override void Verify(Exception exception)
        {
            if (!string.IsNullOrEmpty(_number))
            {
                LogBug();
            }
            if (!SupressError)
            {
                RethrowIfAssertException(exception);
            }
        }

        public void LogBug()
        {
            string msg = $" Expected fail, Bug#= {_number}, Url= {_link}";
            Log(new string('-', msg.Length));
            Log(msg);
            Log(new string('-', msg.Length));
        }
    }
}
