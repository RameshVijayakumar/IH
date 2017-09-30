using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using MoreLinq;

//TODO: Incomplete unit tests
namespace Paycor.Import.Extensions
{
    public static class StringExtensions
    {
        public static string CamelCaseToWords(this string input)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));

            var output = Regex.Replace(input,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                " $1",
                RegexOptions.Compiled);
            if (output.Length > 1) output = output.Substring(0, 1).ToUpper() + output.Substring(1);
            return output;
        }

       
        public static string RemoveWhiteSpaces(this string input)
        {
            return input == null ? null : Regex.Replace(input, @"\s+", "");
        }


        public static string AddBraces(this string input)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            return $"{{{input}}}";
        }

        public static string RemoveBraces(this string input)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            return input.Replace("{", string.Empty).Replace("}", String.Empty);
        }

        public static bool IsLookupParameter(this string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return input.StartsWith("{") && input.EndsWith("}");
        }

        public static int CountofLookupParameters(this string input)
        {
            Ensure.ThatArgumentIsNotNull(input, nameof(input));
            return input.Count(t => t == '{');
        }

        ///employee/v2/clients/{clientId}/employees?employeeNumber={employeeNumber}
        public static string ParseFirstQueryStringParam(this string input)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));

            var length = input.IndexOf("=", StringComparison.Ordinal);

            if (length != -1)
                return input.Substring(length + 1);

            throw new ImportException("Unable to Parse FirstQueryStringParam in the URL");
        }

        public static string ReplaceQueryStringParam(this string input, Dictionary<string, string> replaceValues)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            var length = input.IndexOf("=", StringComparison.Ordinal);
            if (length == -1)
            {
                throw new ImportException("Unable to Replace QueryString Param with Proper Value in the URL");
            }
            var queryString = input.Substring(input.IndexOf("?", StringComparison.Ordinal) + 1).RemoveBraces();
            input = input.Substring(0, input.IndexOf("?", StringComparison.Ordinal));
            foreach (var value in replaceValues)
            {
                queryString = queryString.ReplaceStringIgnoreCase(ImportConstants.Equal + value.Key, ImportConstants.Equal + replaceValues[value.Key]);
            }
            return input + "?" + queryString;
        }


        public static byte[] ToByteArray(this string input)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            return Encoding.UTF8.GetBytes(input);
        }

        public static string ReplaceQuotes(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            const string pattern = @"""(?<pre>.*)""{2}(?<quoted>.*)""{2}(?<suf>.*)""";
            const string replacement = @"${pre}""${quoted}""${suf}";
            var regex = new Regex(pattern, RegexOptions.None);

            return regex.Replace(input, replacement);
        }

        public static bool ContainsDoubleQuotes(this string input)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            return input.Contains(@"""");
        }

        public static string ReplaceRouteParamWithValue(this string input, string routeParam, string replaceValue)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));

            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(routeParam))
                return input;

            if (input.ContainsSubstring(routeParam) && !string.IsNullOrWhiteSpace(replaceValue))
            {
                
                input = input.ReplaceStringIgnoreCase(routeParam, replaceValue.GetFormattedDateifStringIsDate());
            }
            return input;
        }

        public static string GetFormattedDateifStringIsDate(this string input)
        {
            DateTime date;
            if (DateTime.TryParse(input, out date))
            {
                input = input.Replace("/", "-");
            }
            return input;
        }

        public static string IgnoreSameString(this string input, string value)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            Ensure.ThatStringIsNotNullOrEmpty(value, nameof(value));

            if (input == value)
            {
                return input;
            }
            return input + value;
        }

        public static bool HasEndpointPutAndDelete(this string input, string value)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            Ensure.ThatStringIsNotNullOrEmpty(value, nameof(value));

            return Regex.IsMatch(value, @"^(" + input + @"/)(\{[a-z0-9A-Z]{1,}\})$");
        }

        public static List<string> GetFieldsFromBraces(this string input)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));

            var sources = new List<string>();
            var regex = new Regex("{.*?}");
            var matches = regex.Matches(input);
            foreach (var match in matches)
            {
                sources.Add(match.ToString().RemoveBraces());
            }
            return sources;
        }

        public static string RemoveIndexValueFromString(this string input)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));

            string output;
            const int index = 0;
            var elemPos = input.IndexOf("[", index, StringComparison.Ordinal) + 1;
            var elemPos1 = input.IndexOf("]", index, StringComparison.Ordinal);
            var length = elemPos1 - elemPos;
            if (elemPos >= 0 && elemPos1 >= 0)
            {
                var indexValue = input.Substring(elemPos, length);
                int num;
                output = int.TryParse(indexValue, out num)
                    ? $"{input.Substring(0, elemPos)}{input.Substring(elemPos1)}"
                    : input;
            }
            else
            {
                output = input;
            }
            return output;
        }

        public static bool ContainsOpenAndClosedBraces(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return input.Contains("{") && input.Contains("}");
        }

        public static bool ContainsSubstring(this string input, string substring)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            Ensure.ThatStringIsNotNullOrEmpty(substring, nameof(substring));

            return input.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string GetSuffixAfterSymbol(this string input, string symbol)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));
            Ensure.ThatStringIsNotNullOrEmpty(symbol, nameof(symbol));

            var elemPos = input.IndexOf(symbol, StringComparison.Ordinal);
            var suffixPart = elemPos >= 0 ? input.Substring(elemPos + 1) : input;

            return suffixPart;
        }

        public static string ReplaceStringIgnoreCase(this string input, string pattern, string replacement)
        {
            Ensure.ThatStringIsNotNullOrEmpty(input, nameof(input));

            var result = Regex.Replace(input, Regex.Escape(pattern), replacement, RegexOptions.IgnoreCase);
            return result;
        }

        public static string CsvEscape(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var mustQuote = (input.Contains(",") || input.Contains("\"") || input.Contains("\r") || input.Contains("\n"));
            if (!mustQuote) return input;

            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (var nextChar in input)
            {
                sb.Append(nextChar);
                if (nextChar == '"')
                    sb.Append("\"");
            }
            sb.Append("\"");

            return sb.ToString();
        }

        
        public static string FormatFileName(this string fileName, string formatType, string importType, string dateTime = null)
        {
            return dateTime != null ? $"{importType}_{fileName}_{dateTime}{formatType}" : $"{importType}_{fileName}{formatType}";
        }

        public static string FormatFileName(this string fileName,string clientId, string formatType, string importType, string dateTime = null)
        {
            if (string.IsNullOrWhiteSpace(dateTime))
            {
                return string.IsNullOrWhiteSpace(clientId) ? $"{importType}_{fileName}{formatType}" : $"{clientId}_{importType}_{fileName}{formatType}";
            }
            return string.IsNullOrWhiteSpace(clientId) ? $"{importType}_{fileName}_{dateTime}{formatType}" : $"{clientId}_{importType}_{fileName}_{dateTime}{formatType}";
        }

        public static string GetMediaTypeHeaderValue(this string formatType)
        {
            return formatType != ImportConstants.XlsxFileExtension ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        public static bool IsOrdinal(this string input)
        {
            return Regex.IsMatch(input, @"^\d+$");
        }

        public static bool ExistIn(this IEnumerable<string> input1, IEnumerable<string> input2)
        {
            return input1 != null && input1.All(t =>
            {
                return input2 != null && input2.Any(x => string.Compare(x, t, StringComparison.OrdinalIgnoreCase) == 0);
            });
        }

        public static string GetUrlValueWithNoQueryStrings(this string input)
        {
            return input?.Substring(0, input.IndexOf("?", StringComparison.Ordinal));
        }

        public static List<string> GetNotContainedValues(this IList<string> input1, IList<string> input2)
        {
            if (input1 == null) return new List<string>();
            return input2 != null ? input1.Where(t=> !string.IsNullOrWhiteSpace(t)).ExceptBy(input2.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t=> t.ToLower()), t => t.ToLower()).ToList() : new List<string>();
        }

        public static string ConcatListOfString(this IList<string> input, string delimiter)
        {
            return input != null ? input.Where(t=>!string.IsNullOrWhiteSpace(t)).Aggregate((i, j) => i + delimiter + j) : string.Empty;
        }

        public static bool EndsWithAnyInput(this string input, IList<string> input1)
        {
            if (input == null) return false;
            if (input1 == null) return false;
            return input1.Any(input.EndsWith);
        }

        public static IList<string> ReplaceEmptyValuesWithOrdinals(this IList<string> input)
        {
            if (input.Contains(string.Empty) || input.Contains(null))
            {
                var count = -1;
                var result = new List<string>();
                foreach (var field in input)
                {
                    count++;
                    if (string.IsNullOrEmpty(field))
                    {
                        result.Add(count.ToString());
                        continue;
                    }
                    result.Add(field);
                }
                return result;
            }
            return input;
        }

        public static string CheckGreaterThanLength(this string input, int length)
        {
            if (input?.Length > length)
                return null;
            return input;
        }

        public static string[] SplitbyPipe(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? new[] {""} : input.Split(ImportConstants.Pipe);
        }

        public static bool IsConcatenation(this string input)
        {
            var pipe = $"{ImportConstants.Pipe}".ToCharArray();
            return input != null && input.IndexOfAny(pipe) != -1;
        }

        public static string GetFirstValueBeforePipe(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Split(ImportConstants.Pipe).FirstOrDefault();
        }

        public static string[] ExceptFirstValueAfterPipe(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? new[] { "" } : input.Split(ImportConstants.Pipe).Skip(1).ToArray();
        }

        public static int GetNumberofCommaSeperatedData(this string input)
        {
            var commaDelimited = new Regex("(\".*\"|[^\",]+|^)(?=\\s*,|\\s*$)", RegexOptions.Compiled);
            return input != null ? commaDelimited.Matches(input).Count : 0;
        }

        public static string ConcatLookupValuesToLookupMessage(this string message, List<string> lookupValues)
        {
            if (lookupValues == null || !lookupValues.Any()) return message;

            var lookups = lookupValues.ConcatListOfString(",");
            return message + " "+ "(" + lookups + ")";
        }

        public static string ConvertToTrueOrFalse(this string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && input.Trim() == "1")
                input = "True";
            if (!string.IsNullOrWhiteSpace(input) && input.Trim() == "0")
                input = "False";

            return input;
        }

        public static string GetClientGeneratedMappingName(this string input, string clientId)
        {
            return $"{clientId}:{input}";
        }

        public static IEnumerable<string> GetListofPrivilegeClients(this string input)
        {
            if(null == input)
                return new List<string>();
            if(DoesHaveAccessToAllClients(input) || DoesNotHaveAccessToAnyClient(input))
                return new List<string>();

            string[] stringSeparators = { "OR" };

            var seperatedList = input.Split(stringSeparators, StringSplitOptions.None);
            return seperatedList.Select(list => Regex.Replace(list, @"[^\d]", ""))
                                .Where(client => !string.IsNullOrWhiteSpace(client));
        }

        public static bool DoesHaveAccessToAllClients(this string input)
        {
            return input != null && input.Contains("1=1");
        }

        public static bool DoesNotHaveAccessToAnyClient(this string input)
        {
            return input != null && input.Contains("1=0");
        }

        public static string GetLastWordFromSentence(this string input, string delimiter)
        {
            return string.IsNullOrEmpty(input) ? string.Empty : input.Substring(input.LastIndexOf(delimiter, StringComparison.Ordinal) + 1);
        }

        public static string GetPayCorAuthCookie(this IEnumerable<string> input)
        {
            if (input == null) return string.Empty;
            foreach (var cookieValue in input)
            {
                if (cookieValue != null && cookieValue.Contains("paycorAuth"))
                {
                    return cookieValue.Split(';').First().Replace("paycorAuth=", "");
                }
            }
            return string.Empty;
        }

        public static string PrefixWithHttps(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            if (input.Contains("http"))
            {
                return input;
            }

            return "https://" + input;
        }
    }
}