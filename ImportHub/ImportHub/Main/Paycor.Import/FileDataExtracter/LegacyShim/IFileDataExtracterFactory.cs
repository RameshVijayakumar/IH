using log4net;

namespace Paycor.Import.FileDataExtracter.LegacyShim
{
    public interface IFileDataExtracterFactory<in T>
    {
        void LoadHandlers(ILog logger);

        IFileDataExtracter<T> GetFileDataExtracter(string fileName);
    }
}