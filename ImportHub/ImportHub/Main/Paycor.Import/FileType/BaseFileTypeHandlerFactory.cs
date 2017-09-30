using System.Collections.Generic;
using System.Linq;
//TODO: No unit tests
namespace Paycor.Import.FileType
{
    public interface IFileTypeHandlerFactory
    {
        void LoadHandlers();

        IFileTypeHandler GetFileTypeHandler(FileTypeEnum fileType);
    }

    public abstract class BaseFileTypeHandlerFactory : IFileTypeHandlerFactory
    {
        private readonly IList<IFileTypeHandler> _loadedHandlers = new List<IFileTypeHandler>();

        public abstract void LoadHandlers();

        protected void AddFileTypeHandler(IFileTypeHandler fileTypeHandler)
        {
            Ensure.ThatArgumentIsNotNull(fileTypeHandler, nameof(fileTypeHandler));
            _loadedHandlers.Add(fileTypeHandler);
        }

        public IFileTypeHandler GetFileTypeHandler(FileTypeEnum fileType)
        {
            return _loadedHandlers.FirstOrDefault(loaded => loaded.ImportFileType == fileType);
        }
    }

}
