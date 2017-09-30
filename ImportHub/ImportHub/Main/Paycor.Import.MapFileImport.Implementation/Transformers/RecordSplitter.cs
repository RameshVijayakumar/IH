using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Transformers
{
    public class RecordSplitter : IRecordSplitter<MappingDefinition>
    {
        private readonly ILog _log;
        public RecordSplitter(ILog log)
        {
            _log = log;
        }
        public IEnumerable<Dictionary<string, string>> TransformRecordsToDictionaryList(MappingDefinition mappingDefinition, IEnumerable<IEnumerable<KeyValuePair<string, string>>> records)
        {
            var containDuplicateKeys = true;
            var listOfDictionaryRecords = new List<Dictionary<string, string>>();
            var allRecords = records as IEnumerable<KeyValuePair<string, string>>[] ?? records.ToArray();
            foreach (var record in allRecords)
            {
                containDuplicateKeys = record.IsRecordContainDuplicateKey();
                if (containDuplicateKeys)
                {
                    break;
                }
            }
            if (!containDuplicateKeys)
            {
                _log.Info("No duplicate keys found in the entire records");
                
                //if none of the records contains duplicate keys. Simply return in the form of list of dictionary.
                listOfDictionaryRecords = allRecords.Select(record => record.ToDictionary(x => x.Key, x => x.Value)).ToList();
                return listOfDictionaryRecords;
            }
            _log.Info("Dupes keys found in the one or more records.");
            listOfDictionaryRecords = CreateMultipleDictionaryForRecords(allRecords, listOfDictionaryRecords, mappingDefinition).ToList();
            return listOfDictionaryRecords;
        }

        private static IDictionary<string, string> GetRequiredAndLookupFields(
            IEnumerable<KeyValuePair<string, string>> record, MappingDefinition map)
        {
            var requiredFieldItems = new Dictionary<string, string>();
            var keyValuePairsRecords = record as KeyValuePair<string, string>[] ?? record.ToArray();
            var nonDuplicateItems = keyValuePairsRecords.GroupBy(x => x.Key).Where(x => x.Count() == 1) as IGrouping<string, KeyValuePair<string, string>>[] 
                ?? keyValuePairsRecords.GroupBy(x => x.Key).Where(x => x.Count() == 1).ToArray();

            foreach (var fieldDefinations in map.FieldDefinitions)
            {
                foreach (var nonDuplicateItem in nonDuplicateItems)
                {
                    if (fieldDefinations.Destination == nonDuplicateItem.Key && fieldDefinations.Required)
                    {
                        requiredFieldItems[nonDuplicateItem.Key] = nonDuplicateItem.ElementAt(0).Value;
                    }
                    if (fieldDefinations.Destination == nonDuplicateItem.Key.AddBraces() &&
                        fieldDefinations.Destination.IsLookupParameter())
                    {
                        requiredFieldItems[nonDuplicateItem.Key] = nonDuplicateItem.ElementAt(0).Value;
                    }
                }
            }
            return requiredFieldItems;
        }

        private IEnumerable<Dictionary<string, string>> CreateMultipleDictionaryForRecords(IEnumerable<IEnumerable<KeyValuePair<string, string>>> records, 
            List<Dictionary<string, string>> listOfDict, MappingDefinition map)
        {
            try
            {
                foreach (var record in records)
                {
                    var kvpRecord = record as KeyValuePair<string, string>[] ?? record.ToArray();
                    var nonDuplicateItems = kvpRecord.GetNonDuplicateRowItems();
                    var duplicateItems = kvpRecord.GetDuplicateRowItems() as
                        IGrouping<string, KeyValuePair<string, string>>[]
                                         ?? kvpRecord.GetDuplicateRowItems().ToArray();

                    if (duplicateItems.Length == 0)
                    {
                        listOfDict.Add(kvpRecord.ToDictionary(x => x.Key, x => x.Value));
                        continue;
                    }
                    var numberOfDictionaryRequired = duplicateItems.Select(x => x.Count()).Max();
                    listOfDict.Add(CreateFirstDictionary(nonDuplicateItems, duplicateItems).ToDictionary(t=>t.Key,t=>t.Value));
                    listOfDict = CreateNextDictionaries(listOfDict, map, numberOfDictionaryRequired, kvpRecord, duplicateItems).ToList();
                }
            }
            catch (Exception e)
            {
                _log.Error("Error occurred while forming multiple dictionary items for records.",e);
            }
            
            return listOfDict;
        }

        private IEnumerable<Dictionary<string, string>> CreateNextDictionaries(ICollection<Dictionary<string, string>> listOfDict, MappingDefinition map, int numberOfDictionaryRequired,
            KeyValuePair<string, string>[] kvpRecord, IGrouping<string, KeyValuePair<string, string>>[] duplicateItems)
        {
            for (var index = 1; index <= (numberOfDictionaryRequired - 1); index++)
            {
                var nextDictionary = GetRequiredAndLookupFields(kvpRecord, map);
                var count = nextDictionary.Count;
                foreach (var duplicateItem in duplicateItems)
                {
                    if (index > duplicateItem.Count() - 1)
                    {
                        continue;
                    }
                    var value = duplicateItem.ElementAt(index).Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        nextDictionary[duplicateItem.Key] = value;
                    }
                }
                if (nextDictionary.Count <= count)
                {
                    continue;
                }

                _log.Debug("Next dictionary");
                _log.Debug(nextDictionary);

                listOfDict.Add(nextDictionary.ToDictionary(t=>t.Key,t=>t.Value));
            }
            return listOfDict;
        }

        private IDictionary<string, string> CreateFirstDictionary(IEnumerable<IGrouping<string, KeyValuePair<string, string>>> nonDuplicateItems,
                                           IEnumerable<IGrouping<string, KeyValuePair<string, string>>> duplicateItems)
        {
            var firstDictionary = new Dictionary<string, string>();
            foreach (var nonDuplicateItem in nonDuplicateItems)
            {
                firstDictionary[nonDuplicateItem.Key] = nonDuplicateItem.ElementAt(0).Value;
            }
            foreach (var duplicateItem in duplicateItems)
            {
                firstDictionary[duplicateItem.Key] = duplicateItem.ElementAt(0).Value;
            }
            _log.Debug("First dictionary");
            _log.Debug(firstDictionary);

            return firstDictionary;
        }
    }
}
