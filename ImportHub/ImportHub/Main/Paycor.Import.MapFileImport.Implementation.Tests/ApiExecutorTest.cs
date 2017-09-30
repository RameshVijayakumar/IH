using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Sender;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [TestClass]
    public class ApiExecutorTests
    {
        [TestMethod]
        public async Task ApiExecutor_Success()
        {

            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpointAsync(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
            )).Returns(() =>
            {
                var result = new HttpExporterResult

                {
                    Response = new HttpResponseMessage()
                };
                result.Response.Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                    Encoding.UTF8, "application/json");
                return Task.FromResult(result);
            });

            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            generateFailedRecord.Setup(t => t.GetFailedRecord
                    (It.IsAny<ApiRecord>(), It.IsAny<ErrorResponse>(), It.IsAny<HttpExporterResult>()))
                .Returns(new FailedRecord());

            var apiExecutor = new ApiExecutor(logger.Object, httpInvoker.Object, generateFailedRecord.Object);
            try
            {
                var result = await apiExecutor.ExecuteApiAsync(Guid.NewGuid(), new PayloadData(),
                    new Dictionary<string, string>());

                Assert.AreEqual(result.ErrorResultDataItem, null);
            }
            catch (Exception)
            {
                Assert.AreEqual(1, 0);
            }
        }

        [TestMethod]
        public async Task ApiExecutor_BadGateway()
        {

            var logger = new Mock<ILog>();
            var httpInvoker = new Mock<IHttpInvoker>();
            httpInvoker.Setup(t => t.CallApiEndpointAsync(It.IsAny<Guid>(), It.IsAny<string>()
                , It.IsAny<string>(), It.IsAny<HtmlVerb>(), It.IsAny<IDictionary<string, string>>()
            )).Returns(() =>
            {
                var result = new HttpExporterResult

                {
                    Response = new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.BadGateway
                    }
                };
                result.Response.Content = new StringContent(File.ReadAllText("Json\\EmployeeLookupResponse.json"),
                    Encoding.UTF8, "application/json");
                return Task.FromResult(result);
            });

            var generateFailedRecord = new Mock<IGenerateFailedRecord>();
            generateFailedRecord.Setup(t => t.GetFailedRecord
                    (It.IsAny<ApiRecord>(), It.IsAny<ErrorResponse>(), It.IsAny<HttpExporterResult>()))
                .Returns(new FailedRecord());

            var apiExecutor = new ApiExecutor(logger.Object, httpInvoker.Object, generateFailedRecord.Object);
            try
            {
                var result = await apiExecutor.ExecuteApiAsync(Guid.NewGuid(), 
                    new PayloadData {ApiRecord = new ApiRecord()},
                    new Dictionary<string, string>());

                Assert.AreEqual(result.ErrorResultDataItem
                    .HttpExporterResult.Response.StatusCode, HttpStatusCode.BadGateway);
            }
            catch (Exception)
            {
                Assert.AreEqual(1, 0);
            }
        }

    }
}
