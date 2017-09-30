using System.Collections.Specialized;

namespace Paycor.Import.Adapter
{
    public interface ITranslationManager<TInput, TOutput, TResult>
    {
        /// <summary>
        /// The importer to use when importing data to the service
        /// </summary>
        IImporter<TInput> Importer { get; set; }
        /// <summary>
        /// The translator to use when translating the data received
        /// by the importer.
        /// </summary>
        ITranslator<TInput, TOutput> Translator { get; set; }
        /// <summary>
        /// The exporter to use to export the data translated by the
        /// translator
        /// </summary>
        IExporter<TOutput, TResult> Exporter { get; set; }

        /// <summary>
        /// Initializes the translation manager allowing it to load the
        /// classes that it should use. This method will load and initalize
        /// the importer, translator and exporter that will be used by the 
        /// manager.
        /// </summary>
        /// <returns>
        /// True if initialization was successful; otherwise false.
        /// </returns>
        bool Initialize(NameValueCollection configSettings = null);

        /// <summary>
        /// Starts the translation processing, by starting all of the translation 
        /// workers.
        /// </summary>
        /// <returns>
        /// True if all of the workers started otherwise false.
        /// </returns>
        bool Start();

        /// <summary>
        /// Stops the translation processing, by stopping all of the translation 
        /// workers.
        /// </summary>
        /// <returns>
        /// True if all of the workers stopped otherwise false.
        /// </returns>
        bool Stop();
    }
}
