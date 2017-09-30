using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using Paycor.Security.Principal;
using System.Linq;
using Newtonsoft.Json;

namespace Paycor.Import.Service.Service
{
    public class GlobalMapManager : BaseMapManager<GlobalMapping>, IMapOperator

    {
        private readonly ILog _log;
        private readonly PaycorUserPrincipal _principal;
        private readonly IDocumentDbRepository<GlobalMapping> _globalMappingRepository;
        private readonly ITableStorageProvider _tableStorageProvider;
        public GlobalMapManager(ILog log,
            IDocumentDbRepository<GlobalMapping> globalMappingRepository,
            ITableStorageProvider tableStorageProvider): base(globalMappingRepository, log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(tableStorageProvider, nameof(tableStorageProvider));
            Ensure.ThatArgumentIsNotNull(globalMappingRepository, nameof(globalMappingRepository));


            _log = log;
            _principal = HttpContext.Current.User as PaycorUserPrincipal;
            _globalMappingRepository = globalMappingRepository;
            _tableStorageProvider = tableStorageProvider;

        }

        public MapType MapType => MapType.Global;
        public async Task DeleteMapAsync(DeleteMapKeys deleteMapKeys, Dictionary<string, string> errorsList)
        {
            try
            {
                var globalMap = _globalMappingRepository.GetItem(e => e.MappingName.ToLower() == deleteMapKeys.MappingName.ToLower());

                if (globalMap != null)
                {
                    await _globalMappingRepository.DeleteItemAsync(globalMap.Id);

                    var mapAudit = new GenerateMapAudit();
                    _tableStorageProvider.Insert(mapAudit.CreateMapAudit(deleteMapKeys, HtmlVerb.Delete.GetActionFromVerb(), globalMap.Id, _principal.UserName));
                }
                else
                {
                    if (!errorsList.ContainsKey($"GlobalMap:{deleteMapKeys.MappingName}"))
                    {
                        errorsList.Add($"GlobalMap:{deleteMapKeys.MappingName}",
                            $"The requested map {deleteMapKeys.MappingName} is missing or has already been deleted.");
                    }
                }

            }
            catch (Exception ex)
            {
                var message = $"An error occurred while attempting to delete a map:{deleteMapKeys.MappingName}.";
                _log.Error(message, ex);

                if (!errorsList.ContainsKey($"Exception GlobalMap:{deleteMapKeys.MappingName}"))
                {
                    errorsList.Add($"Exception GlobalMap:{deleteMapKeys.MappingName}", message);
                }
            }
        }

        public async Task ConvertMapAsync(ConvertMapKeys convertMapKeys, string map, Dictionary<string, string> errorsList)
        {
            try
            {
                var items = _globalMappingRepository.GetItems(t=> t.MappingName.ToLower() == convertMapKeys.ConvertToMappingName.ToLower());


                if (items.Any() && !errorsList.ContainsKey(convertMapKeys.ConvertToMappingName))
                {
                    errorsList.Add(convertMapKeys.ConvertToMappingName,
                        $"'{convertMapKeys.ConvertToMappingName}' is already in use. Please select different mapping name for the new global map .");
                    return;
                }

                var globalMap = JsonConvert.DeserializeObject<GlobalMapping>(map);

                var deleteId = globalMap.Id;

                globalMap.SystemType = typeof(GlobalMapping).FullName;
                globalMap.Id = string.Empty;
                globalMap.MappingName = convertMapKeys.ConvertToMappingName;

                await SaveMapAsync(JsonConvert.SerializeObject(globalMap));
                await DeleteMapAsync(deleteId);

                var mapAudit = new GenerateMapAudit();
                _tableStorageProvider.Insert(mapAudit.CreateMapAudit(convertMapKeys,
                    HtmlVerb.Delete.GetActionFromVerb(), deleteId, _principal.UserName));

            }
            catch (Exception ex)
            {
                var message = $"An error occurred while attempting to covert a map:{convertMapKeys.ConvertToMappingName}.";
                _log.Error(message, ex);
                if (!errorsList.ContainsKey($"Exception GlobalMap:{convertMapKeys.ConvertToMappingName}"))
                {
                    errorsList.Add($"Exception GlobalMap:{convertMapKeys.ConvertToMappingName}", message);
                }
            }
        }
    }
}