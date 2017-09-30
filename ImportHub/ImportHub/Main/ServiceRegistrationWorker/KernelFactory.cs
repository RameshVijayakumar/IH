using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using log4net;
using log4net.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Ninject;
using Paycor.Import.Azure;
using Paycor.Import.Mapping;
using Paycor.Import.Registration;
using Paycor.Import.Registration.Client;

namespace ServiceRegistrationWorker
{
    public static class KernelFactory
    {
        private static IKernel _kernel;
        [ExcludeFromCodeCoverage]
        public static IKernel CreateKernel(JobHostConfiguration jobHostConfiguration = null)
        {
            if (_kernel != null) return _kernel;
            _kernel = new StandardKernel();

            var config = new JobHostConfiguration
            {
                DashboardConnectionString = ConfigurationManager.AppSettings["StorageConnection"],
                StorageConnectionString = ConfigurationManager.AppSettings["StorageConnection"],
            };
            config.UseServiceBus(new ServiceBusConfiguration
            {
                ConnectionString = ConfigurationManager.AppSettings[RegistrationServiceTopicInfo.ServiceBusConnectionKey],
                MessageOptions = new OnMessageOptions
                {
                    MaxConcurrentCalls = 1
                }
            });

            XmlConfigurator.Configure();
            _kernel.Bind<ILog>()
               .ToMethod(context => LogManager.GetLogger("Service Registration Worker"));
            _kernel.Bind<IMappingCertification>().To<MappingCertification>();
            _kernel.Bind<IMappingFactory>().To<RegistrationMappingGeneratorFactory>();
            _kernel.Bind<ISwaggerParser>().To<SwaggerParser>();
            _kernel.Bind<IMappingsNonPostInfo>().To<MappingsNonPostInfo>();
            _kernel.Bind<IVerifyMaps>().To<MappingLoggingInfo>();
            _kernel.Bind<IVerifyMaps>().To<CertifyMappingRouteParameters>();
            _kernel.Bind<IVerifyMaps>().To<CertifyMappingLookupFields>();
            _kernel.Bind<IGlobalLookupTypeReader>().To<DocumentDbGlobalLookupTypeReader>();
            _kernel.Bind<IApiMappingStatusService>().To<ApiMappingStatusService>();

            _kernel.Bind<IDocumentDbRepository<ApiMappingStatusInfo>>().To<DocumentDbRepository<ApiMappingStatusInfo>>()
                .WithConstructorArgument("database", ConfigurationManager.AppSettings["database"])
                .WithConstructorArgument("collection", ConfigurationManager.AppSettings["docurlcollection"]);

            _kernel.Bind<IDocumentDbRepository<GeneratedMapping>>().To<DocumentDbRepository<GeneratedMapping>>()
                .WithConstructorArgument("database", ConfigurationManager.AppSettings["database"])
                .WithConstructorArgument("collection", ConfigurationManager.AppSettings["collection"]);

            _kernel.Bind<IWebJobProcessor<string>>().To<SwaggerDocRegistrationProcessor>();

            _kernel.Bind<IDocumentDbRepository<GlobalLookupDefinition>>().To<DocumentDbRepository<GlobalLookupDefinition>>()
                .WithConstructorArgument("database", ConfigurationManager.AppSettings["database"])
                .WithConstructorArgument("collection", ConfigurationManager.AppSettings["globallookupcollection"]);

            _kernel.Bind<Functions>().To<Functions>()
                .WithConstructorArgument("configuration", config);

            return _kernel;
        }
    }
}