using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Mapping;

namespace Paycor.Import.Azure
{
    public class DocumentDbGlobalLookupTypeReader : IGlobalLookupTypeReader
    {
        private readonly IDocumentDbRepository<GlobalLookupDefinition> _repository;

        public DocumentDbGlobalLookupTypeReader(IDocumentDbRepository<GlobalLookupDefinition> repository)
        {
            _repository = repository;
        }
        
        public GlobalLookupDefinition LookupDefinition(string lookupType)
        {
            if (string.IsNullOrWhiteSpace(lookupType))
                return null;
            var globalLookup =
                _repository.GetItems(t => t.LookupTypeName.ToLower() == lookupType.ToLower()).FirstOrDefault();

            return globalLookup;
        }

        public IEnumerable<string> GetLookupNames()
        {
            var lookupTypeNames = _repository.GetItems().Select(n => n.LookupTypeName);
            return lookupTypeNames;
        }
    }
}