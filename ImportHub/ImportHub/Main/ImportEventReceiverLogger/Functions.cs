using System;
using log4net;
using Microsoft.Azure.WebJobs;
using Ninject;
using Paycor.Import.Messaging;

namespace ImportEventReceiverLogger
{
    public class Functions
    {
        public static void ProcessQueueMessage([ServiceBusTrigger("importhubevents", "eventmessageverification")]
            FileImportEventMessage message)
        {
            var kernel = KernelFactory.GetKernel();
            var logger = kernel.Get<ILog>();
            logger.Debug($"{nameof(ProcessQueueMessage)} entered.");

            try
            {
                logger.Info(message);
            }
            catch (Exception ex)
            {
                logger.Error("An unexpected exception occurred trying to log an event.", ex);
            }

            logger.Debug($"{nameof(ProcessQueueMessage)} exited.");
        }
    }
}
