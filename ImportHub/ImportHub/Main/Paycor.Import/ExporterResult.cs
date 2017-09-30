using System;
using System.Text;

namespace Paycor.Import
{
    public abstract class ExporterResult
    {
        public string ClientId { get; set; }
        public Exception Exception { get; set; }

        public virtual bool IsSuccess
        {
            get { return (null == Exception); }
        }

        protected ExporterResult(Exception exception = null)
        {
            Exception = exception;
        }

        public String Result
        {
            get
            {
                var result = (IsSuccess) ? BuildResultMessage() 
                                         : BuildErrorMessage();

                return (result);
            }
        }

        protected abstract string BuildResultMessage();

        protected virtual string BuildErrorMessage()
        {
            var e = Exception;
            var sb = new StringBuilder();
            while (null != e)
            {
                sb.AppendLine(e.Message);
                e = e.InnerException;
            }
            return(sb.ToString());
        }
    }
}
