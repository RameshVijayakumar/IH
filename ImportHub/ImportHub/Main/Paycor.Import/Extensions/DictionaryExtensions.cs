using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Messaging;
//TODO: No unit tests
namespace Paycor.Import.Extensions
{
    public static class DictionaryExtensions
    {
        public static HtmlVerb GetVerb(this IDictionary<string, string> record)
        {
            if (!record.ContainsKey(ImportConstants.ActionFieldName)) return HtmlVerb.Post;

            var verb = record[ImportConstants.ActionFieldName];
            record.Remove(ImportConstants.ActionFieldName);
            switch (verb.ToUpper())
            {
                case "C":
                    return HtmlVerb.Put;
                case "A":
                    return HtmlVerb.Post;
                case "D":
                    return HtmlVerb.Delete;
                case "U":
                    return HtmlVerb.Upsert;
                default:
                    throw new InvalidOperationException(verb);
            }
        }

        public static string GetClientId(this IDictionary<string, string> record)
        {
            return (from rec in record
                    where rec.Key.Equals(ImportConstants.Client, StringComparison.OrdinalIgnoreCase)
                    select rec.Value).FirstOrDefault();
        }

        public static void ConcatenateData<T>(this IDictionary<string, IList<T>> input,string key,
            List<T> listData)
        {
            IList<T> previousList = null;

            if(string.IsNullOrWhiteSpace(key))
                return;

            if (input != null && !input.TryGetValue(key, out previousList))
            {
                var newList = new List<T>();
                if (listData != null) newList.AddRange(listData);
                input[key] = newList;
            }
            else
            {
                if (previousList == null || listData == null) return;
                var modifiedList = previousList.ToList();
                modifiedList.AddRange(listData);
                input[key] = modifiedList;
            }
        }

        public static KeyValuePair<HtmlVerb, string> GetEndpointsWithVerbForUpsert(this IReadOnlyDictionary<HtmlVerb, string> endpoints)
        {
            if(endpoints == null || !endpoints.Any()) return new KeyValuePair<HtmlVerb, string>(HtmlVerb.Post, string.Empty);
            var validEndpoints = endpoints.Where(t => !t.Value.ContainsOpenAndClosedBraces() && !string.IsNullOrWhiteSpace(t.Value))
                .ToDictionary(x => x.Key, y => y.Value);
            
            //All lookups have failed (in case of cascading) and there is no valid endpoint to call the API.
            if(!validEndpoints.Any()) return new KeyValuePair<HtmlVerb, string>(HtmlVerb.Post, string.Empty);
            if (validEndpoints.ContainsKey(HtmlVerb.Patch))
            {
                if (!string.IsNullOrWhiteSpace(validEndpoints[HtmlVerb.Patch])) return new KeyValuePair<HtmlVerb, string>(HtmlVerb.Patch, validEndpoints[HtmlVerb.Patch]);
            }
            if (validEndpoints.ContainsKey(HtmlVerb.Put))
            {
                if (!string.IsNullOrWhiteSpace(validEndpoints[HtmlVerb.Put])) return new KeyValuePair<HtmlVerb, string>(HtmlVerb.Put, validEndpoints[HtmlVerb.Put]);
            }
            return new KeyValuePair<HtmlVerb, string>(HtmlVerb.Post, validEndpoints[HtmlVerb.Post]);
        }

        public static IDictionary<string, string> GetDictonaryWithoutEmptyValues(
            this IDictionary<string, string> dictionary)
        {
            return dictionary.Where(t => !string.IsNullOrWhiteSpace(t.Value))
                .ToDictionary(x => x.Key, y => y.Value);
        }

        public static void RemoveAllEmptyDictonaries(
            this List<IDictionary<string, string>> records)
        {
            records.RemoveAll(x => x.Values.All(string.IsNullOrWhiteSpace));
        }
    }
}
