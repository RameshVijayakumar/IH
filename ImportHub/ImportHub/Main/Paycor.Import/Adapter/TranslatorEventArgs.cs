using System;

namespace Paycor.Import.Adapter
{
    /// <summary>
    /// Defines the event arguments that are passed in the ITranslator
    /// TranslateComplete event.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class TranslatorEventArgs<TOutput> : EventArgs
    {
        public TranslatorEventArgs(TOutput output)
        {
            Output = output;
        }

        public TranslatorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public TOutput Output { get; private set; }
        public Exception Exception { get; private set; }
    }

}
