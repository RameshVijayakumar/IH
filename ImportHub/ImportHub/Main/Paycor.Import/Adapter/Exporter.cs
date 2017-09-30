using System;
using System.Threading.Tasks;

namespace Paycor.Import.Adapter
{
    public abstract class Exporter<TData, TResult> : TranslationWorker, IExporter<TData, TResult>
    {
        public event EventHandler<ExporterEventArgs<TData, TResult>> ExportComplete;

        public async Task ExportAsync(TData data)
        {
            ExporterEventArgs<TData, TResult> exporterEventArgs;

            try
            {
                var result = await OnExportAsync(data);
                exporterEventArgs = new ExporterEventArgs<TData, TResult>(data, result);
            }
            catch (Exception exception)
            {
                exporterEventArgs = new ExporterEventArgs<TData, TResult>(data, exception);
            }

            SignalExportComplete(exporterEventArgs);
        }

        /// <summary>
        ///     Sends the specified output result to the registered objects
        /// </summary>
        private void SignalExportComplete(ExporterEventArgs<TData, TResult> exporterEventArgs)
        {
            ExportComplete?.Invoke(this, exporterEventArgs);
        }

        protected abstract Task<TResult> OnExportAsync(TData data);
    }
}