using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Implementation.Shared;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ErrorFormatterTest
    {
        [TestMethod]
        public void FormatError()
        {
            var formatError = new ErrorFormatter();
            var errorResponse = new ErrorResponse
            {
                Source = new Dictionary<string, string>
                  {
                      {"errorkey","errorvalue"}
                  },
                Detail = "This is the detail information for Forbidden error",
                Status = "Forbidden",
                Title = "Title Info"
            };
            var result = new HttpExporterResult
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden
                }
            };
            var actual = formatError.FormatError(errorResponse, result);
            var expected = "This is the detail information for Forbidden error";
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void FormatError_Detail_Null()
        {
            var formatError = new ErrorFormatter();
            var errorResponse = new ErrorResponse
            {
                Source = new Dictionary<string, string>
                  {
                      {"errorkey","errorvalue"}
                  },
                Detail = null,
                Status = "Forbidden",
                Title = "Title Info"
            };
            var result = new HttpExporterResult
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden
                }
            };
            var actual = formatError.FormatError(errorResponse, result);
            var expected = "Title Info";
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void FormatError_Title_Detail_Null()
        {
            var formatError = new ErrorFormatter();
            var errorResponse = new ErrorResponse
            {
                Source = new Dictionary<string, string>
                  {
                      {"errorkey","errorvalue"}
                  },
                Detail = null,
                Status = "Forbidden",
                Title = null
            };
            var result = new HttpExporterResult
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    ReasonPhrase = "The current request is forbidden"
                }
            };
            var actual = formatError.FormatError(errorResponse, result);
            var expected = "The current request is forbidden";
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void FormatError_Title_Detail_Reason_Null()
        {
            var formatError = new ErrorFormatter();
            var errorResponse = new ErrorResponse
            {
                Source = new Dictionary<string, string>
                  {
                      {"errorkey","errorvalue"}
                  },
                Detail = null,
                Status = "Forbidden",
                Title = null
            };
            var result = new HttpExporterResult
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    ReasonPhrase = null
                }
            };
            var actual = formatError.FormatError(errorResponse, result);
            var expected = "Forbidden";
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void FormatError_Title_Detail_Response_Null()
        {
            var formatError = new ErrorFormatter();
            var errorResponse = new ErrorResponse
            {
                Source = new Dictionary<string, string>
                  {
                      {"errorkey","errorvalue"}
                  },
                Detail = null,
                Status = "Forbidden",
                Title = null
            };
            var result = new HttpExporterResult();
            var actual = formatError.FormatError(errorResponse, result);
            
            Assert.IsNull(actual);
        }
    }
}
