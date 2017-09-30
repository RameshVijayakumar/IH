using System;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Azure;
using Paycor.Import.Mapping;

namespace Paycor.Import.Service.Service
{
    public class BaseMapManager<T> where T : ApiMapping, new()
    {
        private readonly IDocumentDbRepository<T> _repository;
        private readonly ILog _log;

        protected BaseMapManager(IDocumentDbRepository<T> repository, ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(repository, nameof(repository));

            _log = log;
            _repository = repository;
        }

        public virtual async Task SaveMapAsync(string rawMap)
        {
            var map = JsonConvert.DeserializeObject<T>(rawMap);
            var addedDocument = await _repository.CreateItemAsync(map);
            if (addedDocument == null)
            {
                throw new Exception($"An error occurred while saving the mapping:{map.MappingName}");
            }
            _log.Info($"Mapping name: {map.MappingName} added.");
        }

        public virtual async Task DeleteMapAsync(string id)
        {
            await _repository.DeleteItemAsync(id);
        }
    }
}