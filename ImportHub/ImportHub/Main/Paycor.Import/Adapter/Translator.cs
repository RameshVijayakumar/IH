using System;

namespace Paycor.Import.Adapter
{
    /// <summary>
    /// A base implementation of the ITranslator interface that provides some basic
    /// functionallity that can be used by derived classes.    
    /// </summary>
    /// <typeparam name="TInput">The type of data the transporter is taking as input</typeparam>
    /// <typeparam name="TOutput">The type of data the transporter will be sending as output/</typeparam>
    public abstract class Translator<TInput, TOutput> : TranslationWorker, ITranslator<TInput, TOutput>
    {
        /// <summary>
        /// Public event that is used to notify the registered object's that
        /// the translation is complete.
        /// </summary>
        public event EventHandler<TranslatorEventArgs<TOutput>> TranslateComplete;

        /// <summary>
        /// Translate the data specified in TInput into TOutput returning
        /// the result through the TranslateComplete event.
        /// </summary>
        public void Translate(TInput input)
        {
            StartTask(() =>
            {
                TranslatorEventArgs<TOutput> translatorEventArgs;

                try
                {
                    var result = OnTranslate(input);
                    translatorEventArgs = new TranslatorEventArgs<TOutput>(result);
                }
                catch (Exception exception)
                {
                    translatorEventArgs = new TranslatorEventArgs<TOutput>(exception);
                }
                SignalTranslateComplete(translatorEventArgs);
            });

        }

        /// <summary>
        /// Sends the specified output result to the registered objects
        /// </summary>
        /// <param name="result">
        /// The translation output to be sent with the complete event.
        /// </param>
        private void SignalTranslateComplete(TranslatorEventArgs<TOutput> result)
        {
            if (TranslateComplete != null)
            {
                TranslateComplete(this, result);
            }
        }

        /// <summary>
        /// Called when the class needs to perform a translation. Derived class
        /// needs to override and perform the actual translation returning the 
        /// result.
        /// </summary>
        /// <param name="input">
        /// The data that needs to be translated.
        /// </param>
        /// <returns>
        /// The resulting translated data.
        /// </returns>
        protected abstract TOutput OnTranslate(TInput input);

    }
}
