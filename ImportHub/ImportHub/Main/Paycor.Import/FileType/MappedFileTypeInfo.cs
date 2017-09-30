using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.FileType
{
    public class MappedFileTypeInfo : FileTypeInfo, IMappedFileTypeInfo
    {
        public List<string> ColumnHeaders { get; set; }
        public bool IsCustomMap { get; set; }
        public int ColumnCount { get; set; }
        public IEnumerable<ApiMapping> AllMappings { get; set; }
        public string Source { get; set; }
        public IEnumerable<ApiMapping> Mappings { get; set; }
        public bool IsMultiSheet { get; set; }
        public IDictionary<string, int?> MapRankings { get; set; }
    }
}