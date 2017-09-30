using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using FluentValidation.WebApi;
using log4net;
using log4net.Config;
using Microsoft.Azure.Documents;
using Microsoft.Owin;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using Owin;
using Paycor.Security.Owin;
using Swashbuckle.Application;
using Paycor.Import.Mapping;
using Paycor.Import.Service;
using Paycor.Import.Azure;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.Employee.ImportHistory;
using Paycor.Import.ImportHistory;
using Paycor.Import.Registration.Client;
using Paycor.Import.Service.Shared;
using Paycor.Import.Status;
using Paycor.Import.Status.Azure;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.FileDataExtracter.LegacyShim;
using Paycor.Import.MapFileImport.Implementation.LegacyShim;
using Paycor.Import.Messaging;
using Paycor.Import.Registration;
using Paycor.Import.Service.Service;
using Paycor.Import.Service.Web;
using Paycor.Import.Validator;
using Paycor.SystemCheck;

[assembly: OwinStartup(typeof(Startup))]

namespace Paycor.Import.Service
{
    public partial class Startup
    {
        private static HttpConfiguration Config { get; set; }

        public void Configuration(IAppBuilder app)
        {
            app.UsePaycorAuth(new PaycorAuthOptions());

            Config = new HttpConfiguration();
            ConfigureSwagger(Config);
            ConfigureIoC(Config);
            ConfigureWebApi(Config);
            app.UseNinjectMiddleware(CreateKernel).UseNinjectWebApi(Config);
            app.UseWebApi(Config);
            FluentValidationModelValidatorProvider.Configure(Config);
            Config.Filters.Add(new ValidationActionFilter());
            Config.Filters.Add(new NoCacheHeaderFilter());
            WebApiStartup.RegisterServiceApi();
        }

        private static StandardKernel CreateKernel()
        {
            var policy = new IndexingPolicy
            {
                IndexingMode = IndexingMode.Consistent,
                Automatic = true,
            };
            policy.IncludedPaths.Add(new IncludedPath
            {
                Path = "/ImportDateEpoch/?",
                Indexes = new Collection<Index> {
                    new RangeIndex(DataType.Number) { Precision = -1 }
                }
            }
            );

            policy.IncludedPaths.Add(new IncludedPath
            {
                Path = "/IsMarkedForDelete/?",
                Indexes = new Collection<Index> {
                    new RangeIndex(DataType.String) { Precision = -1 }
                }
            }
            );

            var databaseId = ConfigurationManager.AppSettings["database"];
            var collection = ConfigurationManager.AppSettings["collection"];
            var globalLookupCollection = ConfigurationManager.AppSettings["globallookupcollection"];
            var importHistoryCollection = ConfigurationManager.AppSettings["importhistorycollection"];
            var importHistoryDb = ConfigurationManager.AppSettings["importhistorydatabase"];

            var connnectionString = ConfigurationManager.AppSettings["BlobStorageConnection"];
            var statusStorageProvider = new BlobStatusStorageProvider(connnectionString, ContainerNames.ImportStatus);
            var kernel = new StandardKernel();
            XmlConfigurator.Configure();
            kernel.Load(Assembly.GetExecutingAssembly());
            kernel.Bind<ILog>()
                .ToMethod(context => LogManager.GetLogger("Import Hub API"));

            kernel.Bind<IFieldMapper>().To<DocumentDbFieldMapper>();

            kernel.Bind<IGlobalLookupTypeReader>().To<DocumentDbGlobalLookupTypeReader>();
           
            kernel.Bind<IDocumentDbRepository<ApiMapping>>().To<DocumentDbRepository<ApiMapping>>()
                .WithConstructorArgument("database", databaseId)
                .WithConstructorArgument("collection", collection);
            kernel.Bind<IDocumentDbRepository<GeneratedMapping>>().To<DocumentDbRepository<GeneratedMapping>>()
                .WithConstructorArgument("database", databaseId)
                .WithConstructorArgument("collection", collection);
            kernel.Bind<IDocumentDbRepository<ClientMapping>>().To<DocumentDbRepository<ClientMapping>>()
                .WithConstructorArgument("database", databaseId)
                .WithConstructorArgument("collection", collection);
            kernel.Bind<IDocumentDbRepository<GlobalMapping>>().To<DocumentDbRepository<GlobalMapping>>()
                .WithConstructorArgument("database", databaseId)
                .WithConstructorArgument("collection", collection);
            kernel.Bind<IDocumentDbRepository<UserMapping>>().To<DocumentDbRepository<UserMapping>>()
                .WithConstructorArgument("database", databaseId)
                .WithConstructorArgument("collection", collection);
            kernel.Bind<IDocumentDbRepository<GlobalLookupDefinition>>()
                .To<DocumentDbRepository<GlobalLookupDefinition>>()
                .WithConstructorArgument("database", databaseId)
                .WithConstructorArgument("collection", globalLookupCollection);
            kernel.Bind<IGlobalLookupTypeWriter>()
                .To<DocumentDbGlobalLookupTypeWriter>()
                .WithConstructorArgument("database", databaseId)
                .WithConstructorArgument("collection", globalLookupCollection);
            kernel.Bind<IDocumentDbRepository<ImportHistoryMessage>>()
                .To<IndexDocumentDbRepository<ImportHistoryMessage>>()
                .WithConstructorArgument("indexingPolicy", policy)
                .WithConstructorArgument("database", databaseId)
                .WithConstructorArgument("collectionId", importHistoryCollection);
            kernel.Bind<ICloudMessageClient<Messaging.FileUploadMessage>>()
                .To<ServiceBusQueueClient<Messaging.FileUploadMessage>>()
                .WithConstructorArgument("retries", GetRetries())
                .WithConstructorArgument("millisecondsToWait", GetWaitTimeMilliseconds());
            kernel.Bind<IStoreData<ImportCancelToken>>()
                .To<ImportCancelTokenStorage>()
                .WithConstructorArgument("storageConnection", connnectionString)
                .WithConstructorArgument("tableName", TableNames.Cancellations)
                .WithConstructorArgument("partitionKey", PartitionKeys.CancelTokenKey);
            

            kernel.Bind<IExcelToCsv>().To<ExcelToCsvConverter>();
            kernel.Bind<IStorageProvider>().To<BlobStorageProvider>();
            kernel.Bind<IXlsxRecordFormatter<string>>().To<XlsxMappingTemplate>();
            kernel.Bind<IFileStorageProvider>().To<FileStorageProvider>();
            kernel.Bind<IStatusStorageProvider>().To<BlobStatusStorageProvider>()
                .WithConstructorArgument("statusStorageProvider", statusStorageProvider);

            kernel.Bind<IImportHistoryService>().To<ImportHistoryService>()
                .WithConstructorArgument("statusStorageProvider", statusStorageProvider);
            kernel.Bind<IValidator<UserMapping>>().To<MappedFileMappingValidator>();
            kernel.Bind<IValidator<ClientMapping>>().To<MappedFileMappingValidator>();
            kernel.Bind<IValidator<GlobalMapping>>().To<MappedFileMappingValidator>();
            kernel.Bind<IValidator<GeneratedMapping>>().To<MappedFileMappingValidator>();
            kernel.Bind<IValidator<ApiMapping>>().To<MappedFileMappingValidator>();
            kernel.Bind<IValidator<MappingDefinition>>().To<NullMappingDefinitionValidator>();
            kernel.Bind<IValidator<MappingDefinition>>().To<NullSourceAndDestinationValidator>();
            kernel.Bind<IProvideSheetData>().To<XlsxDataExtracter>();
            kernel.Bind<IValidator<MappingDefinition>>().To<DuplicateMappingDestinationFieldsValidator>();
            kernel.Bind<IEnvironmentValidator>().To<TopicEnvironmentValidator>();
            kernel.Bind<IEnvironmentValidator>().To<MappedFileTopicEnvironmentValidator>();
            kernel.Bind<IEnvironmentValidator>().To<ServiceBusValidator>();
            kernel.Bind<IEnvironmentValidator>().To<ApiMappingDatabaseEnvironmentValidator>();
            kernel.Bind<IEnvironmentValidator>().To<HistoryDatabaseEnvironmentValidator>();
            kernel.Bind<IEnvironmentValidator>().To<StorageAccountValidator>();
            kernel.Bind<IEnvironmentValidator>().To<DatabaseValidator>();
            kernel.Bind<IEnvironmentValidator>().To<BasicInformationValidator>();
            kernel.Bind<IMappingsNonPostInfo>().To<MappingsNonPostInfo>();
            kernel.Bind<IMappingCertification>().To<MappingCertification>();
            kernel.Bind<ISwaggerParser>().To<SwaggerParser>();
            kernel.Bind<IMappingFactory>().To<RegistrationMappingGeneratorFactory>();
            kernel.Bind<IVerifyMaps>().To<MappingLoggingInfo>();
            kernel.Bind<IVerifyMaps>().To<CertifyMappingLookupFields>();
            kernel.Bind<IVerifyMaps>().To<CertifyMappingRouteParameters>(); 
            kernel.Bind<ITableStorageProvider>().To<TableStorageProvider>();
            kernel.Bind<ILegacyCleanUp>().To<LegacyImportCleanUp>();
            kernel.Bind<IMapService>().To<MapService>();
            kernel.Bind<IMappingManagerFactory>().To<MappingManagerFactory>();
            kernel.Bind<IMapConverter>().To<MapConverter>();

            return kernel;
        }

        private static int GetRetries()
        {
            int retries;
            return int.TryParse(ConfigurationManager.AppSettings["servicebus.retries"], out retries)
                ? retries
                : BaseServiceBusClient<string>.DefaultRetries;
        }

        private static int GetWaitTimeMilliseconds()
        {
            int waitTime;
            return int.TryParse(ConfigurationManager.AppSettings["servicebus.waitTimeMilliseconds"], out waitTime)
                ? waitTime
                : BaseServiceBusClient<string>.DefaultWait;
        }

        private static void ConfigureSwagger(HttpConfiguration config)
        {
            var rootUrl = ConfigurationManager.AppSettings["BaseURL"];
            config
                .EnableSwagger(c =>
                {
                    c.MultipleApiVersions(
                        (apiDesc, targetApiVersion) =>
                        {
                            if (targetApiVersion == "unversioned")
                            {
                                return true;
                            }
                            return apiDesc.RelativePath.Contains(targetApiVersion);
                        },
                        vc =>
                        {
                            vc.Version("v1", "Paycor ImportHub Services (v1)");
                            vc.Version("unversioned", "Paycor ImportHub Services");
                        });
                    c.RootUrl(req => string.IsNullOrEmpty(rootUrl)
                        ? new Uri(req.RequestUri, req.GetRequestContext().VirtualPathRoot).ToString()
                        : $"{rootUrl}/importhubservice");

                    c.ConfigureForImportHub();

                    c.DocumentFilter<FilesPathDocFilter>();
                    c.DocumentFilter(() => new ApiVersionFilter("importhub"));
                    c.IncludeXmlComments(GetXmlCommentsPath());
                })
                .EnableSwaggerUi();
        }


        private static string GetXmlCommentsPath()
        {
            return $"{AppDomain.CurrentDomain.BaseDirectory}\\bin\\Paycor.Import.Service.XML";
        }
    }
}
