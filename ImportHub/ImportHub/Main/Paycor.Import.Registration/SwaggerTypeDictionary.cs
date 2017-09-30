using System.Collections.Generic;

namespace Paycor.Import.Registration
{
    public sealed class SwaggerTypeDictionary
    {
        private static readonly Dictionary<string, string> DotNetTypeFormatLookup;

        static SwaggerTypeDictionary()
        {
            DotNetTypeFormatLookup = new Dictionary<string, string>
            {
                {"string#date-time", "DateTime"},
                {"string#string", "string"},
                {"string#date", "DateTime"},
                {"string#password", "string"},
                {"string#", "string"},
                {"string#byte", "byte"},
                {"string#binary", "byte"},

                {"integer#int16", "short"},
                {"integer#int32", "int"},
                {"integer#int64", "long"},

                {"number#float", "float"},
                {"number#double", "double"},


                {"boolean#", "bool"}
            };

        }

        public static string GetDotNetType(string lookupType)
        {
            string type;

            DotNetTypeFormatLookup.TryGetValue(lookupType.ToLower(), out type);

            return type;
        }
    }
}