using System;
using System.Threading.Tasks;

namespace Paycor.Import.Adapter
{
    public abstract class Importer<TInput> : TranslationWorker, IImporter<TInput>
    {
        /// <summary>
        ///     Public event that is used to notify the registered object's that
        ///     the import is complete.
        /// </summary>
        public event EventHandler<ImporterEventArgs<TInput>> ImportComplete;

        /// <summary>
        ///     Imports the data identified by the specified descriptor
        /// </summary>
        public async Task ImportAsync(dynamic descriptor)
        {
           ImporterEventArgs<TInput> importerEventArgs;

            try
            {
                TInput result = await OnImportAsync(descriptor);
                importerEventArgs = new ImporterEventArgs<TInput>(result);
            }
            catch (Exception exception)
            {
                importerEventArgs = new ImporterEventArgs<TInput>(exception);
            }
            SignalImportComplete(importerEventArgs);
        }

        /// <summary>
        ///     Sends the specified output result to the registered objects
        /// </summary>
        /// <param name="args">
        ///     The importer data to be sent with the complete event.
        /// </param>
        protected void SignalImportComplete(ImporterEventArgs<TInput> args)
        {
            ImportComplete?.Invoke(this, args);
        }

    /// <summary>
        ///     Performs class specific importing
        /// </summary>
        /// <returns>
        ///     The result of the import
        /// </returns>
        protected abstract Task<TInput> OnImportAsync(dynamic descriptor);
    }
}