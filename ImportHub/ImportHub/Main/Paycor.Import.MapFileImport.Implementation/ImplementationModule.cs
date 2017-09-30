using Ninject.Modules;
using Paycor.Import.Azure;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.Employee.ImportHistory;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.FileDataExtracter.LegacyShim;
using Paycor.Import.Http;
using Paycor.Import.ImportHistory;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.FailedRecords;
using Paycor.Import.MapFileImport.Implementation.LegacyShim;
using Paycor.Import.MapFileImport.Implementation.Preparer;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.MapFileImport.Implementation.Sender;
using Paycor.Import.MapFileImport.Implementation.Shared;
using Paycor.Import.MapFileImport.Implementation.Transformers;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using Paycor.Import.Status;
using Paycor.Import.Status.Azure;
using Paycor.Import.Validator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Paycor.Import.Azure.Cache;
using Paycor.Import.Extensions;
using StackExchange.Redis;
using LookupRouteFieldTransformer = Paycor.Import.MapFileImport.Implementation.Transformers.LookupRouteFieldTransformer;
using SourceFieldTransformer = Paycor.Import.MapFileImport.Implementation.Transformers.SourceFieldTransformer;

namespace Paycor.Import.MapFileImport.Implementation
{
    [ExcludeFromCodeCoverage]
    public class ImplementationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IHttpInvoker>().To<HttpInvoker>();
            Bind<IChunkData>().To<MappedFileDataChunker>();
            Bind<IChunkMultiData>().To<MultiSheetDataChunker>();
            Bind<IMappedFileImportImporter>().To<MappedFileImportImporter>()
                .WithConstructorArgument("storageConnectionString", GetStorageConnectionString());
            Bind<IDataSourceBuilder>().To<MappedFileDataSourceBuilder>();
            Bind<IPayloadSenderFactory>().To<SendPayloadFactory>();
            Bind<IPayloadSender>().To<PayloadSender>();
            Bind<IPreparePayload>().To<MappedFilePayloadPreparer>();
            Bind<IStoreData<ImportCancelToken>>().To<ImportCancelTokenStorage>()
                .WithConstructorArgument("storageConnection", GetStorageConnectionString())
                .WithConstructorArgument("tableName", TableNames.Cancellations)
                .WithConstructorArgument("partitionKey", PartitionKeys.CancelTokenKey);
            Bind<IFileDataExtracterFactory<ImportContext>>().To<FileDataExtracterFactory>();
            Bind<ITransformRecordFields<MappingDefinition>>().To<SourceFieldTransformer>();
            
            Bind<ITransformRecordFields<MappingDefinition>>().To<LookupRouteFieldTransformer>();
            Bind<ILookupResolver<MappingFieldDefinition>>().To<LookupResolver>();
            Bind<IRulesEvaluator>().To<LookupRulesEvaluator>();
            Bind<IRecordSplitter<MappingDefinition>>().To<RecordSplitter>();
            Bind<ITransformAliasRecordFields<MappingDefinition>>().To<GlobalAndClientAliasTransformer>()
                .WithConstructorArgument("lookupUri", GetLookUpUri());
            Bind<IFlatToApiRecordsTransformer>().To<FlatToApiRecordsTransformer>();
            Bind<IApiRecordJsonGenerator>().To<ApiRecordJsonGenerator>();
            Bind<Mapping.IRouteParameterFormatter>().To<Mapping.RouteParameterFormatter>();
            Bind<IStorageProvider>().To<BlobStorageProvider>();
            Bind<IStatusStorageProvider>().To<BlobStatusStorageProvider>()
                .WithConstructorArgument("connectionString", GetStorageConnectionString())
                .WithConstructorArgument("rootContainerName", ContainerNames.ImportStatus);
            Bind<IImportHistoryService>().To<ImportHistoryService>()
                .WithConstructorArgument("statusStorageProvider", GetStorageProvider());
            Bind<IFileStorageProvider>().To<FileStorageProvider>();
            Bind<ICalculate>().To<Calculate>();
            Bind<IErrorFormatter>().To<ErrorFormatter>();
            Bind<IProvideClientData<MapFileImportResponse>>().To<ProvideClientData>();
            Bind<IReporter>().To<Reporter.Reporter>();
            Bind<IReportProcessor>().To<ReportProcessor>();
            Bind<IXlsxHeaderWriter<FailedRecord>>().To<XlsxHeaderWriter>();
            Bind<IXlsxRecordWriter<FailedRecord>>().To<XlsxRecordWriter>();
            Bind<IXlsxRecordFormatter<FailedRecord>>().To<XlsxFailedRecordFormatter>();
            Bind<IGenerateFailedRecord>().To<GenerateFailedRecord>();
            Bind<ICloudMessageClient<FileImportEventMessage>>()
                .To<ServiceBusTopicClient<FileImportEventMessage>>()
                .WithConstructorArgument("retries", GetRetries())
                .WithConstructorArgument("millisecondsToWait", GetWaitTimeMilliseconds());
            Bind<IPreparePayloadFactory>().To<PreparePayloadFactory>();
            Bind<IValidator<ApiMapping>>().To<MappedFileMappingValidator>();
            Bind<IValidator<MappingDefinition>>().To<NullMappingDefinitionValidator>();
            Bind<IValidator<MappingDefinition>>().To<NullSourceAndDestinationValidator>();
            Bind<IValidator<MappingDefinition>>().To<DuplicateMappingDestinationFieldsValidator>();
            Bind<IMultiSheetImportImporter>().To<MultiSheetImportImporter>()
                .WithConstructorArgument("storageConnectionString", GetStorageConnectionString());
            Bind<IDocumentDbRepository<ImportHistoryMessage>>().To<DocumentDbRepository<ImportHistoryMessage>>()
                .WithConstructorArgument("database", GetDatabase())
                .WithConstructorArgument("collection", GetCollection());

            BindNotificationClientMessage();

            Bind<ILookup>().To<LookupApiResponse>();
            Bind<IApiExecutor>().To<ApiExecutor>();
            Bind<ICookieResolver>().To<CookieResolver>()
                .WithConstructorArgument("accountsEndpoint", GetAccountsEndpoint())
                .WithConstructorArgument("headers", GetApimKeys());
            Bind<ICacheProvider<Cookie>>().To<RedisCacheProvider<Cookie>>()
                .WithConstructorArgument("cache", Cache)
                .WithConstructorArgument("isRedisConnected", IsRedisConnected);
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

        private static string GetStorageConnectionString()
        {
            return ConfigurationManager.AppSettings["StorageConnection"] ?? string.Empty;
        }

        private static Dictionary<string,string> GetApimKeys()
        {
            var apimKey = ConfigurationManager.AppSettings["APIMKey"];
            return new Dictionary<string, string>
            {
                {ImportConstants.OcpApimKey, apimKey}
            };
        }

        private static string GetLookUpUri()
        {
            return ConfigurationManager.AppSettings["LookupUri"] ?? string.Empty;
        }

        private static BlobStatusStorageProvider GetStorageProvider()
        {
            var connnectionString = GetStorageConnectionString();
            var storageProvider = new BlobStatusStorageProvider(connnectionString, ContainerNames.ImportStatus);

            return storageProvider;
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

        private string GetDatabase()
        {
            var databaseId = ConfigurationManager.AppSettings["importhistorydatabase"];

            if (string.IsNullOrEmpty(databaseId))
            {
                throw new Exception("ImportHistoryDatabase Connection setting is not configured");
            }
            return databaseId;
        }

        private string GetCollection()
        {
            var collectionId = ConfigurationManager.AppSettings["importhistorycollection"];
            if (string.IsNullOrEmpty(collectionId))
            {
                throw new Exception("ImportHistory collection setting is not configured");
            }
            return collectionId;
        }

        private static string GetMessagingEndpoint()
        {
            var messagingUrl = ConfigurationManager.AppSettings["messagingUri"];
            if (string.IsNullOrWhiteSpace(messagingUrl))
            {
                throw new Exception("Messaging Service Endpoint is not configured");
            }
            return messagingUrl.PrefixWithHttps();
        }

        private static string GetAccountsEndpoint()
        {
            var accountsEndpoint = ConfigurationManager.AppSettings["AccountsUri"];
            if (string.IsNullOrWhiteSpace(accountsEndpoint))
            {
                throw new Exception("Accounts Service Endpoint is not configured");
            }
            return accountsEndpoint;
        }

        private void BindNotificationClientMessage()
        {
            var apimOff = ConfigurationManager.AppSettings["TurnOffApim"];
            if (string.IsNullOrWhiteSpace(apimOff) || apimOff.ToLower() == "false")
            {
                Bind<INotificationMessageClient>().To<NotificationMessageClient>()
                    .WithConstructorArgument("messagingEndpoint", GetMessagingEndpoint())
                    .WithConstructorArgument("headers", GetApimKeys());
            }
            else
            {
                Bind<INotificationMessageClient>()
                    .To<MockNotificationMessageClient>()
                    .WithConstructorArgument("messagingEndpoint", GetMessagingEndpoint());
            }
            
        }
    }
}
