using System;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Security;
using Paycor.Security.Principal;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Moq;

namespace Paycor.Import.Tests.Security
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SecurityValidatorTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestClass]
        public class CTorTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SecurityValidator_CTor_Enforce_Log()
            {
                var pup = new PaycorUserPrincipal();
                var securityValidator = new SecurityValidator(null, pup);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SecurityValidator_CTor_Enforce_Principal()
            {
                var log = new Mock<ILog>();
                var securityValidator = new SecurityValidator(log.Object, null);
            }
        }

        [TestClass]
        public class IsUserAuthorizedForEeImportTests
        {
            [TestMethod]
            public void IsUserAuthorizedForEeImport_NullClientId_Fail()
            {
                var log = new Mock<ILog>();
                var pup = new Mock<PaycorUserPrincipal>();
                var securityValidator = new SecurityValidator(log.Object, pup.Object);

                var actual = securityValidator.IsUserAuthorizedForEeImport();
                Assert.IsFalse(actual);
            }

            [TestMethod]
            public void IsUserAuthorizedForEeImport_ValidClientId_Fail()
            {
                const int clientId = 66670;

                var log = new Mock<ILog>();
                var pup = new Mock<PaycorUserPrincipal>();
                var securityValidator = new SecurityValidator(log.Object, pup.Object);

                var actual = securityValidator.IsUserAuthorizedForEeImport(clientId);
                Assert.IsFalse(actual);
            }

            [TestMethod]
            public void IsUserAuthorizedForEeImport_ValidEEImportPrivilegeId_Fail()
            {
                const int clientId = 66670;

                var log = new Mock<ILog>();
                var pup = new Mock<PaycorUserPrincipal>();
                var securityValidator = new SecurityValidator(log.Object, pup.Object);

                var actual = securityValidator.IsUserAuthorizedForEeImport(clientId);
                Assert.IsFalse(actual);
            }

            [TestMethod]
            public void GetUserName_Test()
            {
                var log = new Mock<ILog>();
                var pup = new Mock<PaycorUserPrincipal>();
                var securityValidator = new SecurityValidator(log.Object, pup.Object);

                Assert.AreEqual(" ",securityValidator.GetUserName());
            }
        }
    }
}