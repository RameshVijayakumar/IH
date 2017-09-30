using System;
using System.Threading.Tasks;

namespace Paycor.Import.Adapter
{
    /// <summary>
    ///     Defines the methods and events necessary to implement an
    ///     importer.
    /// </summary>
    /// <typeparam name="TInput">The type of data the translator is taking as input</typeparam>
    public interface IImporter<TInput> : ITranslationWorker
    {
        /// <summary>
        ///     Public event that is used to notify the registered object's that
        ///     the import is complete.
        /// </summary>
        event EventHandler<ImporterEventArgs<TInput>> ImportComplete;

        /// <summary>
        ///     Imports the data identified by the specified descriptor sending the result
        ///     through the ImportComplete event. This descriptor could contain the actual data
        ///     to import, the location of the data to import, or some other information that
        ///     the import can use to retrieve the data.
        /// </summary>
        /// <param name="descriptor">
        ///     Descriptor describing the data to be imported.
        /// </param>
        Task ImportAsync(dynamic descriptor);
    }
}