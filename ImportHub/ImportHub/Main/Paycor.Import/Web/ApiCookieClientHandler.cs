using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Security;
using Paycor.Security.Cookie;
using Paycor.Security.Principal;
//TODO: Missing unit tests

namespace Paycor.Import.Web
{
    public class ApiCookieClientHandler : HttpClientHandler
    {
        private readonly string _baseAddress;
        private readonly Guid _masterSessionId;

        public ApiCookieClientHandler(string baseAddress, Guid masterSessionId)
        {
            Ensure.ThatStringIsNotNullOrEmpty(baseAddress, nameof(baseAddress));

            if (masterSessionId == Guid.Empty)
                throw new ArgumentException("masterSessionId");

            _baseAddress = baseAddress;
            _masterSessionId = masterSessionId;
        }

        public virtual HttpClientHandler GetHttpClientHandler()
        {
            var httpCookie = CreateAuthCookie();
            var handler = CreateHttpClientHandler(httpCookie);

            return handler;
        }

        private HttpClientHandler CreateHttpClientHandler(HttpCookie httpCookie)
        {
            Ensure.ThatArgumentIsNotNull(httpCookie, nameof(httpCookie));

            var baseAddress = new Uri(_baseAddress);
            var netCookie = HttpSecureCookie.ConvertToNetCookie(httpCookie, baseAddress.Host);
            var cookieContainer = new CookieContainer();

            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer
            };

            handler.CookieContainer.Add(netCookie);

            return handler;
        }

        private HttpCookie CreateAuthCookie()
        {
            var puf = new PaycorUserFactory();
            var pup = puf.GetPrincipal(_masterSessionId);

            var name = FormsAuthentication.FormsCookieName;
            const bool secure = false;
            var domain = FormsAuthentication.CookieDomain;
            var path = FormsAuthentication.FormsCookiePath;

            var cookie = pup.MasterSessionID == Guid.Empty
                ? null
                : PaycorUserFactory.CreateAuthCookie(pup, path, name, domain, secure);

            return cookie;
        }
    }
}