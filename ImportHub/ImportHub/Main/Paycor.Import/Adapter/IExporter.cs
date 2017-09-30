using System;
using System.Threading.Tasks;

namespace Paycor.Import.Adapter
{
    public interface IExporter<TData, TResult> : ITranslationWorker
    {
        /// <summary>
        ///     Public event that is used to notify the registered object's that
        ///     the import is complete.
        /// </summary>
        event EventHandler<ExporterEventArgs<TData, TResult>> ExportComplete;

        Task ExportAsync(TData data);
    }
}