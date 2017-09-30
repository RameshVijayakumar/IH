using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using log4net;
using log4net.Config;
using Microsoft.Azure.WebJobs;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Syntax;
using Paycor.Import;
using Paycor.Import.Azure;
using Paycor.Import.Azure.Cache;
using Paycor.Import.Employee.ImportHistory;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.ImportHistory;
using Paycor.Import.Status.Azure;
using Paycor.Import.Messaging;
using StackExchange.Redis;


namespace EmployeeImportWorker
{
    using Importer = Paycor.Import.Adapter.IImporter<FileTranslationData<IDictionary<string, string>>>;
    using Translator = Paycor.Import.Adapter.ITranslator<FileTranslationData<IDictionary<string, string>>, RestApiPayload>;
    using Exporter = Paycor.Import.Adapter.IExporter<RestApiPayload, HttpExporterResult>;
    using FileTranslatorWebJob = FileTranslatorWebJobProcessor<IDictionary<string, string>, RestApiPayload, HttpExporterResult, FileUploadMessage>;

    public static class KernelFactory
    {
        [ExcludeFromCodeCoverage]
        public static IKernel CreateKernel(JobHostConfiguration jobHostConfiguration = null)
        {
            var connnectionString = ConfigurationManager.AppSettings["BlobStorageConnection"];
            var storageProvider = new BlobStatusStorageProvider(connnectionString, ContainerNames.ImportStatus);
            var timeout = ImportConstants.DefaultHttpTimeout;
            var importHistoryCollection = ConfigurationManager.AppSettings["importhistorycollection"];
            var importHistoryDb = ConfigurationManager.AppSettings["importhistorydatabase"];
            var accountsEndpoint = ConfigurationManager.AppSettings["AccountsUri"];

            int result;
            var isGood = int.TryParse(ConfigurationManager.AppSettings["Http.Timeout"], out result);

            if (isGood)
            {
                timeout = result;
            } 
            var kernel = new StandardKernel();

            XmlConfigurator.Configure();

            kernel.Bind<ILog>()
                .ToMethod(context => LogManager.GetLogger("Employee Import WebJob"));
            kernel.Bind<Importer>().To<EmployeeImportImporter>()
                .WithConstructorArgument("storageProvider", storageProvider);
            kernel.Bind<Translator>().To<EmployeeImportTranslator>();
            kernel.Bind<Exporter>().To<EmployeeImportExporter>()
                .WithConstructorArgument("storageProvider", storageProvider)
                .WithConstructorArgument("timeout", timeout);
            kernel.Bind<IWebJobProcessorFactory>().ToFactory();
            kernel.Bind<IWebJobProcessor<FileUploadMessage>>().To<FileTranslatorWebJob>();
            kernel.Bind<IStorageProvider>().To<BlobStorageProvider>();
            kernel.Bind<IDocumentDbRepository<ImportHistoryMessage>>().To<DocumentDbRepository<ImportHistoryMessage>>()
                 .WithConstructorArgument("database", importHistoryDb)
                 .WithConstructorArgument("collection", importHistoryCollection);
            kernel.Bind<IImportHistoryService>().To<ImportHistoryService>()
                .WithConstructorArgument("statusStorageProvider", storageProvider);
            BindNotificationClientMessage(kernel);
            kernel.Bind<IHttpInvoker>().To<HttpInvoker>();
            kernel.Bind<Functions>()
                  .To<Functions>()
                .WithConstructorArgument("configuration",
                    jobHostConfiguration ?? GetJobHostConfiguration());
            kernel.Bind<ICookieResolver>().To<CookieResolver>()
                .WithConstructorArgument("accountsEndpoint", accountsEndpoint)
                .WithConstructorArgument("headers", GetApimKeys());
            kernel.Bind<ICacheProvider<Cookie>>().To<RedisCacheProvider<Cookie>>()
                .WithConstructorArgument("cache", Cache)
                .WithConstructorArgument("isRedisConnected", IsRedisConnected);

            var log = kernel.Get<ILog>();
            log.Debug("Exiting KernalFactory");

            return kernel;
        }

        private static void BindNotificationClientMessage(IBindingRoot kernel)
        {
            var apimOff = ConfigurationManager.AppSettings["TurnOffApim"];
            var messagingEndpoint = ConfigurationManager.AppSettings["messagingUri"];

            if (string.IsNullOrWhiteSpace(apimOff) || apimOff.ToLower() == "false")
            {
                kernel.Bind<INotificationMessageClient>().To<NotificationMessageClient>()
                    .WithConstructorArgument("messagingEndpoint", messagingEndpoint.PrefixWithHttps())
                    .WithConstructorArgument("headers", GetApimKeys());
            }
            else
            {
                kernel.Bind<INotificationMessageClient>()
                    .To<MockNotificationMessageClient>()
                    .WithConstructorArgument("messagingEndpoint", messagingEndpoint);
            }
        }

        private static ConnectionMultiplexer GetRedisConnection()
        {
            try
            {
                var lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["ImportHubRedisConnection"]));
                return lazyConnection.Value;
            }
            catch (Exception)
            {
                // turn off redis caching.
                return null;
            }
        }
        private static readonly IDatabase Cache = GetRedisConnection()?.GetDatabase();
        private static bool IsRedisConnected => Cache?.Multiplexer?.IsConnected ?? false;

        private static Dictionary<string, string> GetApimKeys()
        {
            var apimKey = ConfigurationManager.AppSettings["APIMKey"];
            return new Dictionary<string, string>
            {
                {ImportConstants.OcpApimKey, apimKey}
            };
        }

        private static JobHostConfiguration GetJobHostConfiguration()
        {
            var config = WebJobHelper.GetJobHostConfiguration();
            config.UseServiceBus();
            return config;
        }
    }

}