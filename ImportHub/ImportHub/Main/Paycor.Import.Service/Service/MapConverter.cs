using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Azure;
using Paycor.Import.Mapping;

namespace Paycor.Import.Service.Service
{
    public class MapConverter : IMapConverter
    {
        private readonly ILog _log;
        private readonly IDocumentDbRepository<GlobalMapping> _globalMappingRepository;
        private readonly IDocumentDbRepository<ClientMapping> _clientMappingRepository;
        private readonly IMappingManagerFactory _mappingManagerFactory;


        public MapConverter(ILog log,
            IDocumentDbRepository<ClientMapping> clientMappingRepository,
            IDocumentDbRepository<GlobalMapping> globalMappingRepository,
            IMappingManagerFactory mappingManagerFactory,
            ITableStorageProvider tableStorageProvider)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(tableStorageProvider, nameof(tableStorageProvider));
            Ensure.ThatArgumentIsNotNull(clientMappingRepository, nameof(clientMappingRepository));
            Ensure.ThatArgumentIsNotNull(globalMappingRepository, nameof(globalMappingRepository));
            Ensure.ThatArgumentIsNotNull(mappingManagerFactory, nameof(mappingManagerFactory));

            _log = log;
            _globalMappingRepository = globalMappingRepository;
            _clientMappingRepository = clientMappingRepository;
            _mappingManagerFactory = mappingManagerFactory;
            _mappingManagerFactory.LoadHandlers();
        }
        public async Task<Dictionary<string, string>> ConvertMaps(ConvertMapInfo convertMapInfo)
        {
            var errorDetails = new Dictionary<string, string>();

            foreach (var map in convertMapInfo.Maps)
            {
                if (map.ConvertToMapType == MapType.Client && map.CurrentMapType == MapType.Global)
                {
                    await ConvertGlobalToClientMap(map, errorDetails);
                }

                if (map.ConvertToMapType == MapType.Global && map.CurrentMapType == MapType.Client)
                {
                    await ConvertClientToGlobalMap(map, errorDetails);
                }
            }
            return errorDetails;
        }

        private async Task ConvertGlobalToClientMap(ConvertMapKeys convertMapKeys,
            Dictionary<string, string> errorsList)
        {
            try
            {
                var globalMap =
                    _globalMappingRepository.GetItem(e => e.MappingName.ToLower() ==
                                                          convertMapKeys.CurrentMappingName.ToLower());
                if (globalMap != null)
                {
                    var mappingManager = _mappingManagerFactory.GetMappingManager(convertMapKeys.ConvertToMapType);
                    await mappingManager.ConvertMapAsync(convertMapKeys, JsonConvert.SerializeObject(globalMap),
                        errorsList);
                }
                else
                {
                    var key =
                        $"GlobalMap :{convertMapKeys.CurrentMappingName} is not found, to convert to {convertMapKeys.ConvertToMappingName}:{convertMapKeys.ClientId}";
                    if (!errorsList.ContainsKey(key)) 
                    {
                        errorsList.Add(key,
                            $"The global map {convertMapKeys.CurrentMappingName} is missing and cannot be converted to {convertMapKeys.ConvertToMappingName}:{convertMapKeys.ClientId}");
                    }
                }
            }
            catch (Exception ex)
            {
                var message =
                    $"An error occurred while attempting to covert a map:{convertMapKeys.CurrentMappingName}.";
                _log.Error(message, ex);
                if (!errorsList.ContainsKey($"Exception GlobalMap:{convertMapKeys.CurrentMappingName}"))
                {
                    errorsList.Add($"Exception GlobalMap:{convertMapKeys.CurrentMappingName}", message);
                }
            }
        }

        private async Task ConvertClientToGlobalMap(ConvertMapKeys convertMapKeys,
            Dictionary<string, string> errorsList)
        {
            try
            {
                var clientMap =
                    _clientMappingRepository.GetItem(x => x.ClientId == convertMapKeys.ClientId &&
                                                          x.MappingName.ToLower() == convertMapKeys
                                                              .CurrentMappingName.ToLower());
                if (clientMap != null)
                {
                    var mappingManager = _mappingManagerFactory.GetMappingManager(convertMapKeys.ConvertToMapType);
                    await mappingManager.ConvertMapAsync(convertMapKeys, JsonConvert.SerializeObject(clientMap),
                        errorsList);
                }
                else
                {
                    var key = $"ClientMap :{convertMapKeys.CurrentMappingName}:{convertMapKeys.ClientId} is not found, to convert to {convertMapKeys.ConvertToMappingName}";
                    if (!errorsList.ContainsKey(key))
                    {
                        errorsList.Add(key,
                            $"The client map {convertMapKeys.CurrentMappingName}:{convertMapKeys.ClientId} is missing and cannot be converted to {convertMapKeys.ConvertToMappingName}");
                    }

                }
            }
            catch (Exception ex)
            {
                var message =
                    $"An error occurred while attempting to covert a map:{convertMapKeys.CurrentMappingName}.";
                _log.Error(message, ex);
                if (!errorsList.ContainsKey($"Exception GlobalMap:{convertMapKeys.CurrentMappingName}"))
                {
                    errorsList.Add($"Exception GlobalMap:{convertMapKeys.CurrentMappingName}", message);
                }
            }
        }
    }
}