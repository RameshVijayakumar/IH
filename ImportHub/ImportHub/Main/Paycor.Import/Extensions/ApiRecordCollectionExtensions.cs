using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Messaging;
//TODO: No unit tests
namespace Paycor.Import.Extensions
{
    public static class ApiRecordCollectionExtensions
    {
        public static List<IDictionary<string,string>> AllRecords(this IEnumerable<ApiRecord> apiRecords)
        {
            if (apiRecords == null) throw new ArgumentNullException(nameof(apiRecords));
            return apiRecords.Select(t => t?.Record).ToList();
        }
    }
}
