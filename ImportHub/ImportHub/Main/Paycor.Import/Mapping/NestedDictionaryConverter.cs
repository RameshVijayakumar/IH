using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Paycor.Import.Extensions;

namespace Paycor.Import.Mapping
{
    public class NestedDictionaryConverter : JsonConverter
    {
        private const char ClassDelimiter = '.';
        private readonly string _classDelimiterString = ClassDelimiter.ToString();

        public override bool CanConvert(Type objectType)
        {
            return typeof (IDictionary<string, string>).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dictionary = value as IDictionary<string, string>;
            if (dictionary == null) throw new Exception($"Cannot covert {nameof(value)} to IDictionary<string,string>.");
            Convert(writer, dictionary);
        }

        private void Convert(JsonWriter writer, IDictionary<string, string> dictionary)
        {
            dictionary = dictionary.GetDictonaryWithoutEmptyValues();
            var keys = dictionary.Keys.OrderBy(x => x); 
            var top = keys.Where(x => !x.Contains(_classDelimiterString));
            var nestedGroups = keys.Where(x => x.Contains(_classDelimiterString)).GroupBy(x => x.Split(ClassDelimiter)[0]);

            writer.WriteStartObject();
            foreach (var key in top.Where(key => !string.IsNullOrEmpty(dictionary[key])))
            {
                writer.WritePropertyName(key);
                writer.WriteValue(dictionary[key]);
            }
            foreach (var nestedGroup in nestedGroups)
            {
                writer.WritePropertyName(nestedGroup.Key);

                var nestedDictionary = new Dictionary<string, string>();
                foreach (var keyName in nestedGroup)
                {
                    var propName = keyName.Split(new []{ ClassDelimiter},2)[1];
                    if (!string.IsNullOrEmpty(dictionary[keyName]))
                    {
                        nestedDictionary[propName] = dictionary[keyName];
                    }
                }
                Convert(writer, nestedDictionary);
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}