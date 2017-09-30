using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.FileDataExtracter.LegacyShim;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseFileDataExtracterFactory : IFileDataExtracterFactory<ImportContext>
    {

        private readonly List<IFileDataExtracter<ImportContext>> _loadedFileDataExtracters = new List<IFileDataExtracter<ImportContext>>();

        public abstract void LoadHandlers(ILog logger);


        protected void AddFileDataExtracters(IEnumerable<IFileDataExtracter<ImportContext>> fileDataExtracters)
        {
            _loadedFileDataExtracters.AddRange(fileDataExtracters);
        }


        public IFileDataExtracter<ImportContext> GetFileDataExtracter(string fileName)
        {
            return _loadedFileDataExtracters.FirstOrDefault(t => 
                  fileName != null && fileName.ToLower().EndsWithAnyInput(t.SupportedFileTypes().Split(ImportConstants.Comma).ToList()
                ));
        }
    }
}