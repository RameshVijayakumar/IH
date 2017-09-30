using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Paycor.Import.MapFileImport
{
    [ExcludeFromCodeCoverage]
    public class SubArrayFieldInfo
    {
        public string ArrayName { get; set; }

        public IEnumerable<string> ArrayFieldNames { get; set; }
    }
}