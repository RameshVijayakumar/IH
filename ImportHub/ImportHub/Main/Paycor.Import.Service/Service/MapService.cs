using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Mapping;

namespace Paycor.Import.Service.Service
{
    public class MapService : IMapService 
    {
        private readonly ILog _log;
        private readonly IMappingManagerFactory _mappingManagerFactory;
        private readonly IMapConverter _mapConverter;


        public MapService(ILog log,
            IMappingManagerFactory mappingManagerFactory,
            IMapConverter mapConverter)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(mappingManagerFactory, nameof(mappingManagerFactory));
            Ensure.ThatArgumentIsNotNull(mapConverter, nameof(mapConverter));

            _log = log;
            _mappingManagerFactory = mappingManagerFactory;
            _mappingManagerFactory.LoadHandlers();
            _mapConverter = mapConverter;
        }

        public async Task<Dictionary<string, string>> DeleteMapsAsync(DeleteMapInfo mapinfo)
        {
            var errorsList = new Dictionary<string, string>();

            try
            {
                foreach (var map in mapinfo.Maps)
                {
                    var mappingManager = _mappingManagerFactory.GetMappingManager(map.MapType);
                    await mappingManager.DeleteMapAsync(map, errorsList);
                }
                return errorsList;
            }
            catch (Exception ex)
            {
                var message = "An error occurred while attempting to delete maps.";
                _log.Error(message, ex);

                if (!errorsList.ContainsKey("Exception MapManagerService"))
                {
                    errorsList.Add("MapManagerService", message);
                }
                return errorsList;
            }
        }

        public async Task<Dictionary<string, string>> ConvertMapsAsync(ConvertMapInfo convertMapInfo)
        {
            return await _mapConverter.ConvertMaps(convertMapInfo);
        }
     
    }
}
