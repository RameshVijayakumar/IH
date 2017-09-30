using System.Collections.Generic;
using System.IO;
using Paycor.Import.Mapping;

namespace Paycor.Import.FileDataExtracter.LegacyShim
{
    public interface IFileDataExtracter<in T>
    {
        IList<IDictionary<string, string>> ExtractData(T context, MappingDefinition map, MemoryStream memoryStream);

        string SupportedFileTypes();
    }
}