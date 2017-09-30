using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    public interface IMultiSheetImportImporter
    {
        IList<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>> Import(ImportContext context, IList<ApiMapping> mappings);
    }
}
