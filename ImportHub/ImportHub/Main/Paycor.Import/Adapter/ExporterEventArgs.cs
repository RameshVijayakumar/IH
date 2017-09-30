using System;

namespace Paycor.Import.Adapter
{
    /// <summary>
    ///     Defines the event arguments that are passed in the IExporter
    ///     ExportComplete event.
    /// </summary>
    /// <typeparam name="TData">Type of data returned in the event args</typeparam>
    /// <typeparam name="TResult">The type of the result returned from the export action</typeparam>
    public class ExporterEventArgs<TData, TResult> : EventArgs
    {

        public ExporterEventArgs(TData data, Exception exception)
        {
            Data = data;
            Exception = exception;
        }

        public ExporterEventArgs(TData data, TResult result)
        {
            Data = data;
            Result = result;
        }

        public TData Data { get; private set; }
        public TResult Result { get; private set; }
        public Exception Exception { get; private set; }
    }
}