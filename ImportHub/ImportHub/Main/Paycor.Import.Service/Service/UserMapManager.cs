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

namespace Paycor.Import.Service.Service
{
    public class UserMapManager : BaseMapManager<UserMapping>,IMapOperator
    {
        private readonly ILog _log;
        private readonly PaycorUserPrincipal _principal;
        private readonly IDocumentDbRepository<UserMapping> _userMappingRepository;
        private readonly ITableStorageProvider _tableStorageProvider;
        public UserMapManager(ILog log,
            IDocumentDbRepository<UserMapping> userMappingRepository,
            ITableStorageProvider tableStorageProvider): base(userMappingRepository, log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(tableStorageProvider, nameof(tableStorageProvider));
            Ensure.ThatArgumentIsNotNull(userMappingRepository, nameof(userMappingRepository));


            _log = log;
            _principal = HttpContext.Current.User as PaycorUserPrincipal;
            _userMappingRepository = userMappingRepository;
            _tableStorageProvider = tableStorageProvider;
        }
        public async Task DeleteMapAsync(DeleteMapKeys deleteMapKeys, Dictionary<string, string> errorsList)
        {
            try
            {
                var userMap = _userMappingRepository.GetItem(t =>
                    t.MappingName.ToLower() == deleteMapKeys.MappingName.ToLower() &&
                    t.User == deleteMapKeys.User
                );

                if (userMap != null)
                {
                    await _userMappingRepository.DeleteItemAsync(userMap.Id);

                    var mapAudit = new GenerateMapAudit();
                    _tableStorageProvider.Insert(mapAudit.CreateMapAudit(deleteMapKeys, HtmlVerb.Delete.GetActionFromVerb(), userMap.Id, _principal.UserName));
                }
                else
                {
                    if (!errorsList.ContainsKey($"UserMap:{deleteMapKeys.MappingName}:{deleteMapKeys.User}"))
                    {
                        errorsList.Add($"UserMap:{deleteMapKeys.MappingName}:{deleteMapKeys.User}",
                            $"The requested map {deleteMapKeys.MappingName} is missing or has already been deleted.");
                    }
                }
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while attempting to delete a map:{deleteMapKeys.MappingName}.";
                _log.Error(message, ex);

                if (!errorsList.ContainsKey($"Exception UserMap:{deleteMapKeys.MappingName}:{deleteMapKeys.User}"))
                {
                    errorsList.Add($"Exception UserMap:{deleteMapKeys.MappingName}:{deleteMapKeys.User}", message);
                }
            }

        }

        public Task ConvertMapAsync(ConvertMapKeys convertMapKeys, string map, Dictionary<string, string> errorsList)
        {
            throw new NotImplementedException();
        }

        public MapType MapType => MapType.User;
      
    }
}