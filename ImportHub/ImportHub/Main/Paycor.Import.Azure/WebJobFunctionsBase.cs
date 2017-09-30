using Microsoft.Azure.WebJobs;

namespace Paycor.Import.Azure
{
    public abstract class WebJobFunctionsBase<TBody>
    {
        protected static IWebJobProcessor<TBody> WebJobProcessor { get; set; }
        private readonly JobHost _host;

        protected WebJobFunctionsBase(JobHostConfiguration jobHostConfiguration, IWebJobProcessor<TBody> webJobProcessor = null)
        {
            _host = new JobHost(jobHostConfiguration);
            WebJobProcessor = webJobProcessor;
        }

        public void RunAndBlock()
        {
            _host.RunAndBlock();
        }
    }
}
