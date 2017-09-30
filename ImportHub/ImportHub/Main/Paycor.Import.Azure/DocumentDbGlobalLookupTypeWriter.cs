using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Paycor.Import.Mapping;

namespace Paycor.Import.Azure
{
    public class DocumentDbGlobalLookupTypeWriter : IGlobalLookupTypeWriter
    {
        private readonly IDocumentDbRepository<GlobalLookupDefinition> _documentDbRepository;
        private readonly IDocumentDbRepository<GlobalLookupDefinition> _repository;

        public DocumentDbGlobalLookupTypeWriter(IDocumentDbRepository<GlobalLookupDefinition> documentDbRepository, IDocumentDbRepository<GlobalLookupDefinition> repository)
        {
            Ensure.ThatArgumentIsNotNull(documentDbRepository, nameof(documentDbRepository));
            Ensure.ThatArgumentIsNotNull(repository, nameof(repository));

            _documentDbRepository = documentDbRepository;
            _repository = repository;
        }

        public async Task<Document> CreateLookupDefinition(string lookupTypeName, string lookupKey, string lookupValue)
        {
            if (null == lookupKey)
                return null;

            var item = GlobalLookupDefinition(lookupTypeName);

            Document doc;
            if (item == null)
            {
                doc = await _repository.CreateItemAsync(new GlobalLookupDefinition
                {
                    Id = Guid.NewGuid().ToString(),
                    LookupTypeName = lookupTypeName,
                    LookupTypeValue = new Dictionary<string, string> { { lookupKey, lookupValue } }
                });
                return doc;
            }
            item.LookupTypeValue[lookupKey] = lookupValue;
            doc = await _documentDbRepository.UpsertItemAsync(item);
            return doc;
        }

        public async Task<Document> UpdateLookupDefinition(string lookupTypeName, string lookupKey, string lookupValue)
        {
            var item = GlobalLookupDefinition(lookupTypeName);

            if (item == null) throw new Exception(lookupTypeName + " doesn't exist");
            item.LookupTypeValue[lookupKey] = lookupValue;

            var doc = await _documentDbRepository.UpsertItemAsync(item);
            return doc;
        }

        public async Task<Document> DeleteLookupDefinition(string lookupTypeName, string lookupKey)
        {
            var item = GlobalLookupDefinition(lookupTypeName, lookupKey);

            if (item == null) throw new KeyNotFoundException(lookupKey + " doesn't exist");
            item.LookupTypeValue.Remove(lookupKey);

            var doc = await _documentDbRepository.UpsertItemAsync(item);
            return doc;
        }

        private GlobalLookupDefinition GlobalLookupDefinition(string lookupTypeName, string lookupKey = null)
        {
            if (lookupTypeName == null)
                return null;
            if (string.IsNullOrWhiteSpace(lookupKey))
            {
                return (_documentDbRepository.GetQueryableItems(
                    g => (g.LookupTypeName == lookupTypeName)).ToList()).FirstOrDefault();
            }
            return (_documentDbRepository.GetQueryableItems(
                    g => (g.LookupTypeName == lookupTypeName)).ToList()).FirstOrDefault(
                        g => (g.LookupTypeValue.ContainsKey(lookupKey)));
        }
    }
}
