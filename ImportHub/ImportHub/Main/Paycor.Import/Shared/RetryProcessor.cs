using System;
using System.Threading;
using log4net;

namespace Paycor.Import.Shared
{
    public class RetryProcessor<TInput, TOutput> : IRetryProcessor<TInput, TOutput>
    {
        private readonly ILog _log = LogManager.GetLogger(string.Format("Retry Processor <{0}, {1}>", typeof(TInput).Name, typeof(TOutput).Name));

        #region Properties
        public int RetryCount { get; private set; }
        public TimeSpan RetryInterval { get; private set; }
        public Func<TOutput, bool> SuccessEvaluator { get; private set; }
        #endregion

        public RetryProcessor(Func<TOutput, bool> successEvaluator,
                              int retryCount = 0,
                              TimeSpan retryInterval = default(TimeSpan))
        {
            RetryCount = retryCount;
            RetryInterval = retryInterval;
            SuccessEvaluator = successEvaluator;
        }

        public bool SubmitProcess(Func<TInput, TOutput> process, TInput input, out TOutput output)
        {
            var result = false;
            output = default(TOutput);

            if (null != process)
            {
                var retriesRemaining = RetryCount;

                do
                {
                    try
                    {
                        output = process(input);

                        if (SuccessEvaluator(output))
                        {
                            result = true;
                            break;
                        }
                    }
                    catch
                    {
                        if (0 == retriesRemaining)
                            throw;
                    }

                    if (0 != retriesRemaining)
                    {
                        _log.DebugFormat("{0} was not successful. {1} retries remaining, will retry in {2} minutes, {3} seconds and {4} milliseconds.",
                            process.GetType().Name,
                            retriesRemaining,
                            RetryInterval.Minutes,
                            RetryInterval.Seconds,
                            RetryInterval.Milliseconds);
                        Thread.Sleep(RetryInterval);
                    }


                } while (0 != retriesRemaining--);
            }

            return (result);
        }
    }
}
