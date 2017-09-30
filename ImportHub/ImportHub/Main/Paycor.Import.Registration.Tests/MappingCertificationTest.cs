using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Remoting.Messaging;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Paycor.Import.Registration.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappingCertificationTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void CertifyMappingFailedTest()
        {
            var swaggerText = File.ReadAllText("json\\PayrollService.json");
            var log = new Mock<ILog>();
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);
            var mappingNonPostInfo = new Mock<IMappingsNonPostInfo>();
            var verifyMaps = new Collection<IVerifyMaps>
            {
                new MappingLoggingInfo(log.Object),
                new CertifyMappingRouteParameters(log.Object),
                new CertifyMappingLookupFields(log.Object)
            };
            
           var mappingCertification = new MappingCertification(log.Object, swaggerParser, mappingNonPostInfo.Object, verifyMaps);
           var isMappingValid = mappingCertification.IsAllMappingCertified(swaggerText);
           Assert.AreEqual(false, isMappingValid);            
        }

        [TestMethod]
        public void CertifyMappingPassTest()
        {
            var swaggerText = File.ReadAllText("json\\employeeServiceSwagger.json");
            var log = new Mock<ILog>();
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);
            var mappingNonPostInfo = new Mock<IMappingsNonPostInfo>();
            var verifyMaps = new Collection<IVerifyMaps>
            {
                new MappingLoggingInfo(log.Object),
                new CertifyMappingRouteParameters(log.Object),
                new CertifyMappingLookupFields(log.Object)
            };

            var mappingCertification = new MappingCertification(log.Object, swaggerParser, mappingNonPostInfo.Object, verifyMaps);
            var isMappingValid = mappingCertification.IsAllMappingCertified(swaggerText);
            Assert.AreEqual(true, isMappingValid);
        }

        [TestMethod]
        public void SwaggerEmptyAndNullTest()
        {            
            const string swaggerText = ImportConstants.EmptyStringJsonArray;
            var log = new Mock<ILog>();
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);
            var mappingNonPostInfo = new Mock<IMappingsNonPostInfo>();
            var verifyMaps = new Collection<IVerifyMaps>
                {
                    new MappingLoggingInfo(log.Object),
                    new CertifyMappingRouteParameters(log.Object),
                    new CertifyMappingLookupFields(log.Object)
                };

            var mappingCertification = new MappingCertification(log.Object, swaggerParser, mappingNonPostInfo.Object, verifyMaps);
            var isMappingValid1 = mappingCertification.IsAllMappingCertified(null);
            Assert.AreEqual(false, isMappingValid1);

            var isMappingValid2 = mappingCertification.IsAllMappingCertified(swaggerText);
            Assert.AreEqual(false, isMappingValid2);
        }
    }
}
