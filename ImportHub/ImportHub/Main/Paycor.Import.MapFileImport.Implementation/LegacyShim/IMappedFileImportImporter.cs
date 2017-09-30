using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    public interface IMappedFileImportImporter
    {
        IEnumerable<IDictionary<string, string>> Import(ImportContext context, MappingDefinition map);
    }
}