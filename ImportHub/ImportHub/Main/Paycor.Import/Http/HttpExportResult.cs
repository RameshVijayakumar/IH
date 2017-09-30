using System.Net.Http;

namespace Paycor.Import.Http
{
    public class HttpExporterResult : ExporterResult
    {
        public const string NotAvailable = "Not Available";

        public HttpResponseMessage Response { get; set; }

        public override bool IsSuccess
        {
            get { return ((base.IsSuccess) && 
                          (Response != null) &&
                          (Response.IsSuccessStatusCode)); 
            }
        }

        protected override string BuildResultMessage()
        {
            var result = NotAvailable;

            if (null != Response)
                result = Response.ToString();

            return (result);
        }

        protected override string BuildErrorMessage()
        {
            var result = base.BuildErrorMessage();

            if ((null != Response) &&
                (!Response.IsSuccessStatusCode))
                result = Response.ReasonPhrase;

            return (result);
        }
    }
}
