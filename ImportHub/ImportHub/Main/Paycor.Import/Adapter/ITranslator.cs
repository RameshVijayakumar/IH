using System;

namespace Paycor.Import.Adapter
{
    /// <summary>
    /// Defines the methods and events necessary to implement a
    /// translator.
    /// </summary>
    /// <typeparam name="TInput">The type of data the translator is taking as input</typeparam>
    /// <typeparam name="TOutput">The type of data the translator will be sending as output/</typeparam>
    public interface ITranslator<TInput, TOutput> : ITranslationWorker
    {
        /// <summary>
        /// Public event that is used to notify the registered object's that
        /// the translation is complete.
        /// </summary>
        event EventHandler<TranslatorEventArgs<TOutput>> TranslateComplete;

        /// <summary>
        /// Translate the data specified in TInput into TOutput returning
        /// the result through the TranslateComplete event.
        /// </summary>
        void Translate(TInput input);

    }
}
