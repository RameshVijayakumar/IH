using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.FileType
{
    public interface IMappedFileTypeInfo : IFileTypeInfo
    {
        bool IsCustomMap { get; set; }
        IEnumerable<ApiMapping> AllMappings { get; set; }
        List<string> ColumnHeaders { get; set; }
        int ColumnCount { get; set; }
        string Source { get; set; }
        IEnumerable<ApiMapping> Mappings { get; set; }
        bool IsMultiSheet { get; set; }
    }
}