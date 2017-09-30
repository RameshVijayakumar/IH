using System;
using System.Collections.Specialized;

namespace Paycor.Import.Adapter
{

    /// <summary>
    /// Defines the methods and properties necessary to implement a worker for use
    /// with the translation manager
    /// </summary>
    public interface ITranslationWorker : IDisposable
    {
        /// <summary>
        /// Initializes the translation worker using the given configuration settings
        /// </summary>
        /// <param name="configSettings">
        /// Configuration settings containing the worker's initialization data
        /// </param>
        /// <returns>
        /// True if initialization was successful; otherwise false
        /// </returns>
        bool Initialize(NameValueCollection configSettings);
        /// <summary>
        /// Starts the worker allowing it to perform it's functions
        /// </summary>
        /// <returns>
        /// True if the worker has started; otherwise false
        /// </returns>
        bool Start();
        /// <summary>
        /// Stops the worker waiting for it's processing to complete.
        /// </summary>
        /// <returns>
        /// True if the worker has stopped; otherwise false.
        /// </returns>
        bool Stop();
    }
}
