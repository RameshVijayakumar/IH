using System;
using System.Collections.Generic;
using System.Linq;
//TODO: Missing unit tests
// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.Extensions
{
    public static class KeyValuePairExtensions
    {
        public static bool IsRecordContainDuplicateKey(this IEnumerable<KeyValuePair<string, string>> record)
        {
            var duplicateKeyValuePair = record.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1);
            return duplicateKeyValuePair.Any();
        }

        public static IEnumerable<IGrouping<string, KeyValuePair<string, string>>> GetNonDuplicateRowItems(
            this IEnumerable<KeyValuePair<string, string>> record)
        {
            var nonDuplicateItems = record.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() == 1);
            return nonDuplicateItems;
        }

        public static IEnumerable<IGrouping<string, KeyValuePair<string, string>>> GetDuplicateRowItems(
            this IEnumerable<KeyValuePair<string, string>> record)
        {
            var duplicateItems = record.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1);
            return duplicateItems;
        }

        public static bool RecordContainsKey(this IEnumerable<KeyValuePair<string, string>> record, string key)
        {
            return record.Any(rec => string.Equals(rec.Key,key,StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<KeyValuePair<string, string>> GetRecordsWithFormattedExceptionMessage( this List<KeyValuePair<string, string>> record,
            List<string> exceptionMessages)
        {
            if (exceptionMessages.Count == 0)
            {
                return record;
            }
            var exceptionMessage = string.Join(ImportConstants.Newline, exceptionMessages.ToArray());
            var item = new KeyValuePair<string, string>($"{ImportConstants.LookUpRouteExceptionMessageKey}", exceptionMessage);
            if (record.RecordContainsKey(item.Key))
            {
                record.RemoveAll(x => x.Key.Equals(item.Key));
            }
            record.Add(item);
            return record;
        }

        public static List<KeyValuePair<string, string>> AddOrOverwriteKeyValuePair(
           this List<KeyValuePair<string, string>> record, KeyValuePair<string, string> keyValuePair)
        {
            if (keyValuePair.Key != null && record != null && record.RecordContainsKey(keyValuePair.Key))
            {
                record.RemoveAll(x=> x.Key.Equals(keyValuePair.Key, StringComparison.OrdinalIgnoreCase));
            }
           record?.Add(keyValuePair);
           return record;
        }

        public static List<KeyValuePair<string, string>> AddOrOverwriteKeyValuePair(
           this List<KeyValuePair<string, string>> record, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                record.AddOrOverwriteKeyValuePair(keyValuePair);
            }

            return record;
        }

        public static string ConcatenateRecordFields(this List<KeyValuePair<string, string>>
            records, IEnumerable<string> concatenateFields)
        {
            return concatenateFields.Aggregate(string.Empty, (current, fieldName) => current + records.Find(t => t.Key == fieldName).Value);
        }

        public static void AddUpsertIfActionisNotPresent(this List<KeyValuePair<string, string>> records)
        {
            var actionPresent = records.FirstOrDefault(t => t.Key == ImportConstants.ActionFieldName);
            if (string.IsNullOrWhiteSpace(actionPresent.Key))
                records.Add(new KeyValuePair<string, string>(ImportConstants.ActionFieldName, ImportConstants.UpsertAction));
        }
    }
}
