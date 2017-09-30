using System;
using System.Net.Http;

namespace Paycor.Import.Web
{
    public interface IWebApiClientGet
    {
        string Get(string uri,
            int retries = 0,
            TimeSpan retryInterval = default(TimeSpan),
            Func<HttpResponseMessage, bool> successEvaluator = null);
    }
}