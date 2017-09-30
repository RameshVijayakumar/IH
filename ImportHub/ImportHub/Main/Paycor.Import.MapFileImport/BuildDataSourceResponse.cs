using System.Diagnostics.CodeAnalysis;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport
{
    [ExcludeFromCodeCoverage]
    public class BuildDataSourceResponse : MapFileImportResponse
    {
        public IChunkDataSource DataSource { get; set; }

        public string ImportType { get; set; }
    }
}
