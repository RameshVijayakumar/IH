using System;
using log4net;
using Microsoft.Azure.WebJobs;
using Paycor.Import;
using Paycor.Import.Azure;
using Paycor.Import.Registration.Client;


namespace ServiceRegistrationWorker
{
    public class Functions : WebJobFunctionsBase<string>
    {
        private static ILog _log;
        private static IWebJobProcessor<string> _processor;

        public Functions(ILog log, IWebJobProcessor<string> processor, JobHostConfiguration configuration)
            : base(configuration, _processor)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(processor, nameof(processor));
            Ensure.ThatArgumentIsNotNull(configuration, nameof(configuration));

            _log = log;
            _processor = processor;
        }

        public static void ProcessRestfulServiceRegistration(
            [ServiceBusTrigger(RegistrationServiceTopicInfo.TopicName, RegistrationServiceTopicInfo.SubscriptionName)]
            string docUrl)
        {
            try
            {
                _processor.Process(docUrl);
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(ProcessRestfulServiceRegistration)}", ex);
            }
        }
    }
}
