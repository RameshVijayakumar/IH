using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Mapping;

namespace Paycor.Import.Service.Service
{
    public class MappingManagerFactory : IMappingManagerFactory
    {
        private readonly ILog _log;
        private readonly IDocumentDbRepository<ClientMapping> _clientMappingRepository;
        private readonly IDocumentDbRepository<GlobalMapping> _globalMappingRepository;
        private readonly IDocumentDbRepository<UserMapping> _userMappingRepository;
        private readonly ITableStorageProvider _tableStorageProvider;
        private readonly List<IMapOperator> _loadedGenerators = new List<IMapOperator>();

        public MappingManagerFactory(ILog log, IDocumentDbRepository<ClientMapping> clientMappingRepository,
            IDocumentDbRepository<GlobalMapping> globalMappingRepository,
            IDocumentDbRepository<UserMapping> userMappingRepository,
            ITableStorageProvider tableStorageProvider)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(tableStorageProvider, nameof(tableStorageProvider));
            Ensure.ThatArgumentIsNotNull(clientMappingRepository, nameof(clientMappingRepository));
            Ensure.ThatArgumentIsNotNull(globalMappingRepository, nameof(globalMappingRepository));
            Ensure.ThatArgumentIsNotNull(userMappingRepository, nameof(userMappingRepository));

            _log = log;
            _clientMappingRepository = clientMappingRepository;
            _globalMappingRepository = globalMappingRepository;
            _userMappingRepository = userMappingRepository;
            _tableStorageProvider = tableStorageProvider;
        }

        public void LoadHandlers()
        {
            _loadedGenerators.AddRange(
                new List<IMapOperator>
                {
                    new GlobalMapManager(_log,_globalMappingRepository,_tableStorageProvider),
                    new ClientMapManager(_log,_clientMappingRepository,_tableStorageProvider),
                    new UserMapManager(_log,_userMappingRepository,_tableStorageProvider)
                });
        }

        public IMapOperator GetMappingManager(MapType? maptype)
        {
            return _loadedGenerators.FirstOrDefault(loaded => loaded.MapType == maptype);
        }
    }
}