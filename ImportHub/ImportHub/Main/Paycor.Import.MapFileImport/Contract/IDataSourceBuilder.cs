using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IDataSourceBuilder
    {
        BuildDataSourceResponse Build(ImportContext context, ApiMapping mapping, IEnumerable<IDictionary<string, string>> chunk);
    }
}
