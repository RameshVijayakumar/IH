using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using Paycor.Security.Principal;

namespace Paycor.Import.Service.Service
{
    public class ClientMapManager : BaseMapManager<ClientMapping>, IMapOperator
    {
        private readonly ILog _log;
        private readonly PaycorUserPrincipal _principal;
        private readonly IDocumentDbRepository<ClientMapping> _clientMappingRepository;
        private readonly ITableStorageProvider _tableStorageProvider;
        public ClientMapManager(ILog log,
            IDocumentDbRepository<ClientMapping> clientMappingRepository,
            ITableStorageProvider tableStorageProvider) : base(clientMappingRepository,log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(tableStorageProvider, nameof(tableStorageProvider));
            Ensure.ThatArgumentIsNotNull(clientMappingRepository, nameof(clientMappingRepository));


            _log = log;
            _principal = HttpContext.Current.User as PaycorUserPrincipal;
            _clientMappingRepository = clientMappingRepository;
            _tableStorageProvider = tableStorageProvider;

        }
        public MapType MapType => MapType.Client;
        public async Task DeleteMapAsync(DeleteMapKeys deleteMapKeys, Dictionary<string, string> errorsList)
        {
            try
            {
                var clientMap = _clientMappingRepository.GetItem(t =>
                    t.ClientId == deleteMapKeys.ClientId &&
                    t.MappingName.ToLower() == deleteMapKeys.MappingName.ToLower()
                );

                if (clientMap != null)
                {
                    await _clientMappingRepository.DeleteItemAsync(clientMap.Id);

                    var mapAudit = new GenerateMapAudit();
                    _tableStorageProvider.Insert(mapAudit.CreateMapAudit(deleteMapKeys, HtmlVerb.Delete.GetActionFromVerb(), clientMap.Id, _principal.UserName));
                }
                else
                {
                    if (!errorsList.ContainsKey($"ClientMap:{deleteMapKeys.MappingName}:{deleteMapKeys.ClientId}"))
                    {
                        errorsList.Add($"ClientMap:{deleteMapKeys.MappingName}:{deleteMapKeys.ClientId}",
                            $"The requested map {deleteMapKeys.MappingName} is missing or has already been deleted.");
                    }
                }
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while attempting to delete a map:{deleteMapKeys.MappingName}.";
                _log.Error(message, ex);
                if (!errorsList.ContainsKey($"Exception ClientMap:{deleteMapKeys.MappingName}:{deleteMapKeys.ClientId}"))
                {
                    errorsList.Add($"Exception ClientMap:{deleteMapKeys.MappingName}:{deleteMapKeys.ClientId}", message);
                }
            }
        }

        public async Task ConvertMapAsync(ConvertMapKeys convertMapKeys,string map, Dictionary<string, string> errorsList)
        {
            try
            {
                var items = _clientMappingRepository.GetItems(x => x.ClientId == convertMapKeys.ClientId &&
                                                                   x.MappingName.ToLower() == convertMapKeys
                                                                       .ConvertToMappingName.ToLower());


                if (items.Any() && !errorsList.ContainsKey(convertMapKeys.ConvertToMappingName))
                {
                    errorsList.Add(convertMapKeys.ConvertToMappingName,
                        $"'{convertMapKeys.ConvertToMappingName}' is already in use. Please select different mapping name for the new client map .");
                    return;
                }

                var clientMap = JsonConvert.DeserializeObject<ClientMapping>(map);

                var deleteId = clientMap.Id;

                clientMap.ClientId = convertMapKeys.ClientId;
                clientMap.SystemType = typeof(ClientMapping).FullName;
                clientMap.Id = string.Empty;
                clientMap.MappingName = convertMapKeys.ConvertToMappingName;

                await SaveMapAsync(JsonConvert.SerializeObject(clientMap));
                await DeleteMapAsync(deleteId);

                var mapAudit = new GenerateMapAudit();
                _tableStorageProvider.Insert(mapAudit.CreateMapAudit(convertMapKeys,
                    HtmlVerb.Delete.GetActionFromVerb(), deleteId, _principal.UserName));

            }
            catch (Exception ex)
            {
                var message = $"An error occurred while attempting to covert a map:{convertMapKeys.ConvertToMappingName}.";
                _log.Error(message, ex);
                if (!errorsList.ContainsKey($"Exception ClientMap:{convertMapKeys.ConvertToMappingName}"))
                {
                    errorsList.Add($"Exception ClientMap:{convertMapKeys.ConvertToMappingName}", message);
                }
            }
     
        }
    }
}