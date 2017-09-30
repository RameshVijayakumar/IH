using System;

namespace Paycor.Import.Shared
{
    /// <summary>
    /// This interface defines the properties and methods necessary to
    /// implement a retry processor. A retry processor accepts a process
    /// that shoould be retried a number of times if it fails.
    /// </summary>
    /// <typeparam name="TInput">
    /// The type of data that is submitted to the running process.
    /// </typeparam>
    /// <typeparam name="TOutput">
    /// The type of data that is returned from the process if it
    /// completes sucessfully.
    /// </typeparam>
    public interface IRetryProcessor<TInput, TOutput>
    {
        /// <summary>
        /// This method will execute the specified process, retrying if it
        /// fails. 
        /// </summary>
        /// <param name="process">
        /// The process that should be executed and retried should it fail.
        /// </param>
        /// <param name="input">
        /// The data that should be sent to the process.
        /// </param>
        /// <param name="output">
        /// The result of the process method.
        /// </param>
        /// <returns>
        /// True should the process succeed with output being set to the processes
        /// result. If all retries are exhausted before the process succeeds then
        /// false is returned and output will be set to the last response received.
        /// </returns>
        bool SubmitProcess(Func<TInput, TOutput> process, TInput input, out TOutput output);
    }
}
