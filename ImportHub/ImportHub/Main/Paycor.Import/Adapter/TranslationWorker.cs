using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Paycor.Import.Adapter
{
    /// <summary>
    /// A base implementation of the ITranslationWorker that provides some basic
    /// functionallity that can be used by derived classes.
    /// </summary>
    public abstract class TranslationWorker : ITranslationWorker
    {
        protected bool Disposed { get; set; }
        protected Task WorkerTask { get; set; }

        /// <summary>
        /// Initializes the translation worker using the given configuration settings
        /// </summary>
        /// <param name="configSettings">
        /// Configuration settings containing the worker's initialization data
        /// </param>
        /// <returns>
        /// True if initialization was successful; otherwise false
        /// </returns>
        public virtual bool Initialize(NameValueCollection configSettings)
        {
            return (true);
        }

        /// <summary>
        /// Starts the worker allowing it to perform it's functions
        /// </summary>
        /// <returns>
        /// True if the worker has started; otherwise false
        /// </returns>
        public virtual bool Start()
        {
            return (true);
        }

        /// <summary>
        /// Stops the worker waiting for it's processing to complete.
        /// </summary>
        /// <returns>
        /// True if the worker has stopped; otherwise false.
        /// </returns>
        public virtual bool Stop()
        {
            return (true);
        }

        protected void StartTask(Action task)
        {
            WorkerTask = Task.Run(task);
        }

        /// <summary>
        /// Waits for the Task to complete execution within a specified number of milliseconds.
        /// </summary>
        /// <param name="millisecondsTimeout">Number of millseconds to wait, or Infinite (-1) to wait indefinitely</param>
        /// <returns>
        /// true if the Task completed execution within the allotted time; otherwise, false.
        /// </returns>
        public bool WaitForTask(int millisecondsTimeout)
        {
            bool result = true;

            try
            {
                if (null != WorkerTask)
                    result = WorkerTask.Wait(millisecondsTimeout);
            }
            catch (ObjectDisposedException)
            {
            }

            return(result);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Disposed = true;
        }
    }
}
