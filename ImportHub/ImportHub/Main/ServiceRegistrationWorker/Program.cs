using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Ninject;
using Paycor.Import;
using Newtonsoft.Json;
using Paycor.Messaging.AppRegistration.Client;
using Paycor.Messaging.Contract.v1;
using Paycor.Import.Azure;
using map = Paycor.Import.Mapping;
using AutoMapper;
using Paycor.Import.Registration;

namespace ServiceRegistrationWorker
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        private static IKernel _kernel;
        private const string MigrationVersionId = "ImportHub-5.0";
        private const string ApplicationId = "ImportHub";

        private static void Main()
        {
            _kernel = KernelFactory.CreateKernel();

            var functions = _kernel.Get<Functions>();
            var log = _kernel.Get<ILog>();

            Task.Run(async () =>
            {
                await PerformDocDbMigration(log);
                await RegisterApplicationAndMessageTypes(log);
                await UpdateDescriptionForGeneratedMappings(log);
            }).GetAwaiter().GetResult();

            functions.RunAndBlock();
        }

        private class MigrationIndicator : RepositoryObject
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("migrationLevel")]
            public string MigrationLevel { get; set; }

            [JsonProperty("databaseObjectType")]
            public string DatabaseObjectType { get; set; }
        }

        private static async Task RegisterApplicationAndMessageTypes(ILog log)
        {
            var paycorServiceBusConnection = ConfigurationManager.AppSettings["PaycorServiceBusConnection"];

            var messageTypeRegistrations = new List<MessageTypeRegistration>
            {
                new MessageTypeRegistration
                {
                    ApplicationId = ApplicationId,
                    Category = "General",
                    Description = "Sends a notification when a file import has been completed.",
                    Name = NotificationTypes.ImportCompletion,
                    SupportedDestinations = new[]
                    {
                        DeliveryProviderTypeEnum.Email,
                        DeliveryProviderTypeEnum.Text,
                    },
                    Privileges = new List<int> { PrivilegeConstants .EmployeeImportPrivilegeId}
                    //ShouldRemove = true
                },
            };

            var client = new RegisterMessagingApplicationClient(paycorServiceBusConnection);
            await client.RegisterApplication(
                new ApplicationRegistration
                {
                    Name = ApplicationId,
                    Description = "Import Hub Applications",
                    Priority = ApplicationPriorityEnum.Medium,
                    Contacts = new List<Contact>
                    {
                        new Contact
                        {
                            Name = "Alan Pimm",
                            ContactType = ContactTypeEnum.Technical,
                            Email = "apimm@paycor.com"
                        },
                        new Contact
                        {
                            Name = "Ann Diep",
                            ContactType = ContactTypeEnum.Business,
                            Email = "adiep@paycor.com"
                        },
                        new Contact
                        {
                            Name = "Atul Paradkar",
                            ContactType = ContactTypeEnum.Portfolio,
                            Email = "aparadkar@paycor.com"
                        }
                    },
                    //ShouldRemove = true
                },
                messageTypeRegistrations
            );
            log.Info("Created message type registrations");
        }

        private static async Task PerformDocDbMigration(ILog log)
        {
 
            var databaseName = ConfigurationManager.AppSettings["database"];
            var collectionName = ConfigurationManager.AppSettings["collection"];

            DocumentDbRepository<MigrationIndicator> migrationIndicator;
            MigrationIndicator currentVersion;

            if (IsDocDbUptoDate(databaseName, collectionName, log, out migrationIndicator, out currentVersion))
                return;
            log.Info($"Current version info: {currentVersion}.");
            log.Info($"Need to upgrade ImportHistoryMessage, Current Version is:{currentVersion.MigrationLevel}");

            try
            {
                await MigrateApiMappings(databaseName, collectionName, log);
                log.Info("Successfully upgraded ApiMappings");
                await UpdateMigrationLevel(currentVersion, migrationIndicator);
                log.Info($"Successfully updated MigrationLevel for ImportHistoryMessage:{currentVersion.MigrationLevel}");
            }
            catch (Exception ex)
            {
                log.Error("Upgrade of ApiMappings in database failed", ex);
            }
        }

        private static async Task UpdateMigrationLevel(MigrationIndicator currentVersion, 
            IDocumentDbRepository<MigrationIndicator> migrationIndicator)
        {
            if (currentVersion == null)
            {
                currentVersion = new MigrationIndicator
                {
                    MigrationLevel = MigrationVersionId,
                };
            }
            else
            {
                currentVersion.MigrationLevel = MigrationVersionId;
            }
            await migrationIndicator.UpsertItemAsync(currentVersion);
        }

        private static async Task MigrateApiMappings(string databaseName, string collectionName, ILog log)
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<LegacyApiMapping, map.UserMapping>();
                cfg.CreateMap<LegacyApiMapping, map.GeneratedMapping>();
                cfg.CreateMap<LegacyApiMapping, map.GlobalMapping>();
            });
            var mapper = config.CreateMapper();

            var legacyMappingRepository = new DocumentDbRepository<LegacyApiMapping>(databaseName, collectionName, log);
            var apiMappingRepository = new DocumentDbRepository<map.GeneratedMapping>(databaseName, collectionName, log);
            var userApiMappingRepository = new DocumentDbRepository<map.UserMapping>(databaseName, collectionName, log);
            var globalApiMappingRepository = new DocumentDbRepository<map.GlobalMapping>(databaseName, collectionName, log);

            // Get all of the existing mappings using the existing GeneratedMapping type, but deserialize into LegacyApiMapping to perform upgrade
            var allMappings = legacyMappingRepository.GetItemsFromSystemType(typeof(map.ApiMapping).FullName);
            foreach (var mapping in allMappings)
            {
                if (!string.IsNullOrEmpty(mapping.User))
                {
                    // Save as UserMapping
                    var userMapping = mapper.Map<LegacyApiMapping, map.UserMapping>(mapping);
                    await userApiMappingRepository.UpsertItemAsync(userMapping);
                }
                else if (string.IsNullOrEmpty(mapping.DocUrl))
                {
                    // Save as GlobalMapping
                    var globalMapping = mapper.Map<LegacyApiMapping, map.GlobalMapping>(mapping);
                    await globalApiMappingRepository.UpsertItemAsync(globalMapping);
                }
                else
                {
                    // Save as GeneratedMapping
                    var apiMapping = mapper.Map<LegacyApiMapping, map.GeneratedMapping>(mapping);
                    await apiMappingRepository.UpsertItemAsync(apiMapping);
                }
            }
        }


        private static async Task UpdateDescriptionForGeneratedMappings(ILog log)
        {
            try
            {

                var databaseName = ConfigurationManager.AppSettings["database"];
                var collectionName = ConfigurationManager.AppSettings["collection"];

                var generatedMappingRepository = new DocumentDbRepository<map.GeneratedMapping>(databaseName, collectionName, log);

                var generatedMappings = generatedMappingRepository.GetItemsFromSystemType(typeof(map.GeneratedMapping).FullName);
                foreach (var mapping in generatedMappings)
                {
                    if (!string.IsNullOrEmpty(mapping.MappingDescription)) continue;
                    mapping.MappingDescription = MappingDescription.GetMappingDescription(mapping.MappingName);
                    await generatedMappingRepository.UpsertItemAsync(mapping);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception occurred at {nameof(Main)}:{nameof(UpdateDescriptionForGeneratedMappings)}", ex);
            }
        }


        private static bool IsDocDbUptoDate(string databaseName, string collectionName, ILog log,
            out DocumentDbRepository<MigrationIndicator> migrationIndicator, out MigrationIndicator currentVersion)
        {
            migrationIndicator = new DocumentDbRepository<MigrationIndicator>(databaseName, collectionName, log);
            currentVersion =
                migrationIndicator.GetItems().FirstOrDefault();
            if (currentVersion == null)
            {
                currentVersion = new MigrationIndicator
                {
                    MigrationLevel = "0"
                };
                return false;
            }
            if (currentVersion.MigrationLevel != MigrationVersionId) return false;
            log.Info($"Current Version for ApiMappings:{currentVersion.MigrationLevel}");
            return true;
        }
    }
}
