using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using static System.Console;


namespace Paycor.Import.ImportHubTest.Common
{
    public static class Utils
    {
        public static void Log<T>( T message, bool indent = false)
        {
            var time = $"{DateTime.Now.ToLongTimeString()}: ";
            Write(time);
            var padding = indent ? new string(' ', time.Length) : time; 
            if (typeof(T) == typeof(string))
            {
                WriteLine($"{time}{message}");
            }else if(typeof(T) == typeof(Array))
            {
                WriteLine($"{time}{string.Join(",", message)}");
            }
        }

        public static bool CompareString(this string source, string target, ItStringCompareOptions option)
        {
            switch (option)
            {
                case ItStringCompareOptions.Exact:
                    return string.Compare(source, target, StringComparison.OrdinalIgnoreCase) == 0;
                case ItStringCompareOptions.StartWith:
                    return source.StartsWith(target);
                case ItStringCompareOptions.EndWith:
                    return source.EndsWith(target);
                case ItStringCompareOptions.Contains:
                    return source.Contains(target);
                default:
                    return false;
            }
        }

        public enum ItStringCompareOptions
        {
            StartWith,
            Exact,
            EndWith,
            Contains
        }

        public static string Serialize<T>(this T obj)
        {
            return obj == null ? null : JsonConvert.SerializeObject(obj);
        }

        public static List<T> TryDeserialize<T>(this HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            try
            {
                return new List<T>() {JsonConvert.DeserializeObject<T>(content)};
            }
            catch (JsonSerializationException)
            {
                return DeserializeList<T>(content);
            }
        }
        public static IEnumerable<T> Deserialize<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
        }

        public static List<T> DeserializeList<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<List<T>>(jsonString);
        }

        public static JObject TryParseJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return null;
            }
            else
            {
                return JObject.Parse(jsonString);
            }
        }

        public static bool IsNotNullOrEqualToExpectation(this string source, string expected)
        {
            return !String.IsNullOrEmpty(source) && String.Compare(source, expected, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool ValidateDefaultOrExpected<T>(this T source, T expected)
        {
            if (source == null)
                return false;
            return EqualityComparer<T>.Default.Equals(source, expected);
        }

        public static string MaskString(string message, int index)
        {
            return $"{message.Substring(0, index)}######";
        }

        public static IEnumerable<string> GetExcelSheetNames(string filename)
        {
            List<string> names = new List<string>();
            try
            {
                using (var file = new FileStream(filename, FileMode.Open))
                {
                    var excel = new ExcelPackage(file);
                    names.AddRange(excel.Workbook.Worksheets.Select(s => s.Name));
                }
            }
            catch (Exception)
            {
                Log($"Error occured when parse file {filename}");
                throw;
            }
            return names;
        }
    }
}
