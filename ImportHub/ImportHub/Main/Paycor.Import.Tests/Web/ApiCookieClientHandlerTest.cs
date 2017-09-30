using System;
using System.Net.Http;
using System.Web;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Web;
using Paycor.Security.Cookie;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Moq;

namespace Paycor.Import.Employee.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApiCookieClientHandlerTest
    {
        private const string BaseAddress = "http://localhost";
        private string _name;
        private string _domain;
        private string _path;
        private bool _secure;
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }


        [TestInitialize]
        public void InitializeTest()
        {
            _name = FormsAuthentication.FormsCookieName;
            _secure = false;
            _domain = FormsAuthentication.CookieDomain;
            _path = FormsAuthentication.FormsCookiePath;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ApiCookieClientHandler_CTor_Enforce_MasterSessionId()
        {
            var masterGuid = new Guid();
            var apiHandler = new ApiCookieClientHandler(BaseAddress, masterGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ApiCookieClientHandler_CTor_Enforce_BaseAddress()
        {
            var masterSessionId = Guid.NewGuid();
            var apiHandler = new ApiCookieClientHandler(null, masterSessionId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApiCookieClientHandler_GetHttpClientHandler_MasterSessionID_Invalid()
        {
            var masterSessionId = Guid.NewGuid();

            var apiHttpClientHandler =
                new ApiCookieClientHandler(BaseAddress, masterSessionId);

            var actual = apiHttpClientHandler.GetHttpClientHandler();

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ApiCookieClientHandler_GetHttpClientHandler_Handler_Valid()
        {
            var baseUri = new Uri(BaseAddress);
            var masterSessionId = Guid.NewGuid();
            var httpCookie = new HttpCookie("sessionid")
            {
                Name = _name,
                Domain = _domain,
                Path = _path,
                Secure = _secure
            };

            var netCookie = HttpSecureCookie.ConvertToNetCookie(httpCookie, baseUri.Host);

            var httpHandler = new Mock<HttpClientHandler>();
            httpHandler.Object.CookieContainer.Add(netCookie);

            var mockApiCookieClientHandler = new Mock<ApiCookieClientHandler>(BaseAddress, masterSessionId)
            {
                CallBase = true
            };

            var actual = mockApiCookieClientHandler.Setup(r => r.GetHttpClientHandler())
                .Returns(httpHandler.Object);

            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void ApiCookieClientHandler_CreateAuthCookie_MasterSessionID_Invalid()
        {
            var masterSessionId = Guid.NewGuid();
            var apiHttpClientHandler = new PrivateObject(typeof(ApiCookieClientHandler), BaseAddress, masterSessionId);

            var actual =
                (HttpCookie)apiHttpClientHandler.Invoke("CreateAuthCookie");

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApiCookieClientHandler_CreateHttpClientHandler_Cookie_Null()
        {
            HttpCookie httpCookie = null;
            var masterSessionId = Guid.NewGuid();

            var apiHttpClientHandler = new PrivateObject(typeof(ApiCookieClientHandler), BaseAddress, masterSessionId);

            var actual =
                (HttpClientHandler)apiHttpClientHandler.Invoke("CreateHttpClientHandler", httpCookie);

            Assert.IsNull(actual);
        }

    }

}