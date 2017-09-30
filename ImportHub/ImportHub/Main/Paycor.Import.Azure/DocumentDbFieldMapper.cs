using System;
using System.Collections.Generic;
using Paycor.Import.Mapping;
using System.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.FileType;
using Paycor.Security.Principal;

// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.Azure
{
    public class DocumentDbFieldMapper : IFieldMapper
    {
        private readonly IDocumentDbRepository<GeneratedMapping> _generatedMappingRepository;
        private readonly IDocumentDbRepository<ClientMapping> _clientMappingRepository;
        private readonly IDocumentDbRepository<GlobalMapping> _globalMappingRepository;
        private readonly IDocumentDbRepository<UserMapping> _userMappingRepository;
        private readonly RelevanceAlgorithmFactory _factory;

        public DocumentDbFieldMapper(IDocumentDbRepository<GeneratedMapping> generatedMappingRepository,
            IDocumentDbRepository<ClientMapping> clientMappingRepository,
            IDocumentDbRepository<GlobalMapping> globalMappingRepository,
            IDocumentDbRepository<UserMapping> userMappingRepository)
        {
            Ensure.ThatArgumentIsNotNull(generatedMappingRepository, nameof(generatedMappingRepository));
            Ensure.ThatArgumentIsNotNull(clientMappingRepository, nameof(clientMappingRepository));
            Ensure.ThatArgumentIsNotNull(globalMappingRepository, nameof(globalMappingRepository));
            Ensure.ThatArgumentIsNotNull(userMappingRepository, nameof(userMappingRepository));

            _generatedMappingRepository = generatedMappingRepository;
            _clientMappingRepository = clientMappingRepository;
            _globalMappingRepository = globalMappingRepository;
            _userMappingRepository = userMappingRepository;

            _factory = new RelevanceAlgorithmFactory();
            _factory.LoadHandlers();
        }
        public ApiMapping FindMappingDefinition(IEnumerable<string> fields, PaycorUserPrincipal principal)
        {
            IDictionary<string, int?> mapRankings;
            return GetMappingDefinitions(fields, principal, out mapRankings).FirstOrDefault();
        }

        public IEnumerable<ApiMapping> GetMappingDefinitions(IEnumerable<string> fields,
            PaycorUserPrincipal principal,
            out IDictionary<string, int?> mapRankings,
            string objectType = null,
            AlgorithmType algorithmType = AlgorithmType.Legacy,
            FileTypeSortOrder sortOrder = FileTypeSortOrder.Alpha)
        {
            Ensure.ThatArgumentIsNotNull(fields, nameof(fields));

            var allMappings = GetAllApiMappings(principal, objectType);
            var algorithm = _factory.GetAlgorithm(algorithmType);
            if (algorithm == null) throw new Exception("factory get for algorithm failed.");
            return algorithm.GetMappingsFor(allMappings, fields, sortOrder, out mapRankings);
        }

        public IEnumerable<ApiMapping> GetAllApiMappings(PaycorUserPrincipal principal, string objectType = null)
        {
            var userKey = principal?.UserKey.ToString();

            var mappings = new List<ApiMapping>();

            var generatedMappings = _generatedMappingRepository.GetItemsFromSystemType(typeof(GeneratedMapping).ToString()).ToList();
            var clientMappings = _clientMappingRepository.GetItemsFromSystemType(typeof(ClientMapping).ToString()).ToList();
            var globalMappings = _globalMappingRepository.GetItemsFromSystemType(typeof(GlobalMapping).ToString()).ToList();
            var userMappings = _userMappingRepository.GetItemsFromSystemType(typeof(UserMapping).ToString()).ToList();

            generatedMappings.UpdateGeneratedMappingName();
            clientMappings.UpdateGeneratedMappingName();
            globalMappings.UpdateGeneratedMappingName();
            userMappings.UpdateGeneratedMappingName();

            var rls = principal?.GetRowLevelSecurity(PrivilegeConstants.AbilityToLaunchPerform);
            var clients = new List<string>();
            if (rls != null)
            {
                clients = rls.GetListofPrivilegeClients().ToList();
            }

            if (null == objectType)
            {
                mappings.AddRange(generatedMappings);
                mappings.AddRange(rls != null && rls.DoesHaveAccessToAllClients()
                    ? clientMappings
                    : clientMappings.Where(t => clients.Contains(t.ClientId)));

                mappings.AddRange(globalMappings);
                mappings.AddRange(userMappings.Where(t=>t.User == userKey));
            }
            else
            {
                mappings.AddRange(generatedMappings.Where(t=>t.ObjectType == objectType));
                mappings.AddRange(rls != null && rls.DoesHaveAccessToAllClients()
                    ? clientMappings.Where(t => t.ObjectType == objectType)
                    : clientMappings.Where(t => t.ObjectType == objectType && clients.Contains(t.ClientId)));
                mappings.AddRange(globalMappings.Where(t => t.ObjectType == objectType));
                mappings.AddRange(userMappings.Where(t => t.User == userKey && t.ObjectType == objectType));
            }
            return mappings;
        }
    }
}
