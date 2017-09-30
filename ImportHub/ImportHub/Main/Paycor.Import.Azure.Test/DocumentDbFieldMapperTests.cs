using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Paycor.Import.Mapping;
using Paycor.Security.Principal;

namespace Paycor.Import.Azure.Test
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DocumentDbFieldMapperTests
    {
        private Mock<IDocumentDbRepository<GeneratedMapping>> _repo;
        private Mock<IDocumentDbRepository<ClientMapping>> _clientRepo;
        private Mock<IDocumentDbRepository<GlobalMapping>> _globalRepo;
        private Mock<IDocumentDbRepository<UserMapping>> _userRepo;
        private Mock<PaycorUserPrincipal> _pup;

        public DocumentDbFieldMapperTests()
        {
            var stringVal = File.ReadAllText("MockRepos.json");

            var mappings = JsonConvert.DeserializeObject<IEnumerable<GeneratedMapping>>(stringVal);
            var items = mappings.AsQueryable();

            var clientMappings = JsonConvert.DeserializeObject<IEnumerable<ClientMapping>>(stringVal);
            var clientItems = clientMappings.AsQueryable();

            var globalMappings = JsonConvert.DeserializeObject<IEnumerable<GlobalMapping>>(stringVal);
            var globalItems = globalMappings.AsQueryable();

            var userMappings = JsonConvert.DeserializeObject<IEnumerable<UserMapping>>(stringVal);
            var userItems = userMappings.AsQueryable();

            _repo = new Mock<IDocumentDbRepository<GeneratedMapping>>();
            _clientRepo = new Mock<IDocumentDbRepository<ClientMapping>>();
            _globalRepo = new Mock<IDocumentDbRepository<GlobalMapping>>();
            _userRepo = new Mock<IDocumentDbRepository<UserMapping>>();


            _repo.Setup(e => e.GetItemsFromSystemType(It.IsAny<string>())).Returns(items.Where(t=>t.SystemType == typeof(GeneratedMapping).ToString()));

            _clientRepo.Setup(e => e.GetItemsFromSystemType(It.IsAny<string>())).Returns(clientItems.Where(t => t.SystemType == typeof(ClientMapping).ToString()));
            _globalRepo.Setup(e => e.GetItemsFromSystemType(It.IsAny<string>())).Returns(globalItems.Where(t => t.SystemType == typeof(GlobalMapping).ToString()));
            _userRepo.Setup(e => e.GetItemsFromSystemType(It.IsAny<string>())).Returns(userItems.Where(t => t.SystemType == typeof(UserMapping).ToString()));


            _pup = new Mock<PaycorUserPrincipal>();
        }

        [TestMethod]
        public void GetAllMappings()
        {
            Assert.IsNotNull(_repo);
            Assert.IsNotNull(_pup);
            var mapper = new DocumentDbFieldMapper(_repo.Object,_clientRepo.Object,_globalRepo.Object,_userRepo.Object);
            var mappings = mapper.GetAllApiMappings(_pup.Object);
            Assert.AreEqual(9, mappings.Count());
        }

        [TestMethod]
        public void GetAllMappingsWithNullPup()
        {
            var mapper = new DocumentDbFieldMapper(_repo.Object, _clientRepo.Object, _globalRepo.Object, _userRepo.Object);
            var mappings = mapper.GetAllApiMappings(null);
            Assert.AreEqual(7, mappings.Count());
        }

        [TestMethod]
        public void GetAllMappingsWithObjectType()
        {
            Assert.IsNotNull(_repo);
            Assert.IsNotNull(_pup);
            var mapper = new DocumentDbFieldMapper(_repo.Object, _clientRepo.Object, _globalRepo.Object, _userRepo.Object);
            var mappings = mapper.GetAllApiMappings(_pup.Object, "Employee");
            Assert.AreEqual(3, mappings.Count());
        }
    }
}
