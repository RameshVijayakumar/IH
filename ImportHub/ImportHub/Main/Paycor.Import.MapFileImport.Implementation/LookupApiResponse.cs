using System.Collections.Generic;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation
{
    public class LookupApiResponse : ILookup
    {
        private readonly Dictionary<string, string> _lookupData;

        public LookupApiResponse()
        {
            _lookupData = new Dictionary<string, string>();
        }
        public void Store(string key, string value)
        {
            _lookupData[key] = value;
        }

        public string Retrieve(string key)
        {
            string value;
            _lookupData.TryGetValue(key, out value);

            return value;
        }

        public void Remove(string key)
        {
            _lookupData.Remove(key);
        }
    }
}