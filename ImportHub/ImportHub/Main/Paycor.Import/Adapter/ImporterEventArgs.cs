using System;

namespace Paycor.Import.Adapter
{
    /// <summary>
    /// Defines the event arguments that are passed in the IImporter
    /// ImportComplete event.
    /// </summary>
    /// <typeparam name="TInput">The type of data that will be imported</typeparam>
    public class ImporterEventArgs<TInput> : EventArgs
    {
        public ImporterEventArgs(TInput data)
        {
            Data = data;
        }

        public ImporterEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public TInput Data { get; private set; }
        public Exception Exception { get; private set; }
    }
}
