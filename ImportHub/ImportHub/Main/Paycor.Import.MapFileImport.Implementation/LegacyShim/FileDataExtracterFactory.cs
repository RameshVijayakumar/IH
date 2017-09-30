using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using log4net;
using Paycor.Import.FileDataExtracter.LegacyShim;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    [ExcludeFromCodeCoverage]
    public class FileDataExtracterFactory : BaseFileDataExtracterFactory
    {
        public override void LoadHandlers(ILog logger)
        {
            AddFileDataExtracters(
                new List<IFileDataExtracter<ImportContext>>
                {
                    new CsvDataExtracter(logger),
                    new XlsxDataExtracter()
                });
        }
    }
}