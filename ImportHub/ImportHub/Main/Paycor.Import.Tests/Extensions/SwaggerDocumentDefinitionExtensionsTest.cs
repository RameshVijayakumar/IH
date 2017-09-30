using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SwaggerDocumentDefinitionExtensionsTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }
        
        [TestMethod]
        public void FormatEndPoint_For_Http()
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            };


            var swaggerDefinition = JsonConvert.DeserializeObject<SwaggerDocumentDefinition>(File.ReadAllText(".\\Json\\HttpGamesSwaggerV2.json"), settings);
            var endpoint = swaggerDefinition.FormatEndPoint("/import/referenceapi/v2/formula1/drivers");

            Assert.AreEqual("http://localhost:54911/import/referenceapi/v2/formula1/drivers", endpoint);
        }

        [TestMethod]
        public void FormatEndPoint_For_Https()
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            };


            var swaggerDefinition = JsonConvert.DeserializeObject<SwaggerDocumentDefinition>(File.ReadAllText(".\\Json\\HttpsGamesSwaggerV2.json"), settings);
            var endpoint = swaggerDefinition.FormatEndPoint("/import/referenceapi/v2/formula1/drivers");

            Assert.AreEqual("https://localhost:54911/import/referenceapi/v2/formula1/drivers", endpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void FormatEndPoint_For_InvalidScheme()
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            };


            var swaggerDefinition = JsonConvert.DeserializeObject<SwaggerDocumentDefinition>(File.ReadAllText(".\\Json\\InvalidSchemeGamesSwaggerV2.json"), settings);
            swaggerDefinition.FormatEndPoint("/import/referenceapi/v2/formula1/drivers");
        }

        [TestMethod]
        public void FormatEndPoint_IgnoringBasePath__For_Http()
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            };


            var swaggerDefinition = JsonConvert.DeserializeObject<SwaggerDocumentDefinition>(File.ReadAllText(".\\Json\\employeeServiceSwagger.json"), settings);
            var endpoint = swaggerDefinition.FormatEndPointIgnoringBasePath("/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}");

            Assert.AreEqual("http://localhost/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}", endpoint);
        }
    }
}
