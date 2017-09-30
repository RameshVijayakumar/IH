using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Web;

namespace Paycor.Import.Registration
{
    public class SwaggerDocRegistrationProcessor : IWebJobProcessor<string>
    {
        private readonly IDocumentDbRepository<GeneratedMapping> _serviceRegistrationRepository;
        private readonly IGlobalLookupTypeReader _globalLookupTypeReader;
        private readonly ILog _log;
        private readonly ISwaggerParser _swaggerParser;
        private readonly IApiMappingStatusService _apiMappingStatusService;
        private readonly IMappingCertification _mappingCertification;
     
        public SwaggerDocRegistrationProcessor(IDocumentDbRepository<GeneratedMapping> serviceRegistrationRepository,
            ILog log, IGlobalLookupTypeReader globalLookupTypeReader, ISwaggerParser swaggerParser, IApiMappingStatusService apiMappingStatusService,
            IMappingCertification mappingCertification)
        {
            Ensure.ThatArgumentIsNotNull(serviceRegistrationRepository, nameof(serviceRegistrationRepository));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(globalLookupTypeReader, nameof(globalLookupTypeReader));
            Ensure.ThatArgumentIsNotNull(swaggerParser, nameof(swaggerParser));
            Ensure.ThatArgumentIsNotNull(apiMappingStatusService, nameof(apiMappingStatusService));
            Ensure.ThatArgumentIsNotNull(mappingCertification, nameof(mappingCertification));


            _serviceRegistrationRepository = serviceRegistrationRepository;
            _log = log;
            _globalLookupTypeReader = globalLookupTypeReader;
            _swaggerParser = swaggerParser;      
            _apiMappingStatusService = apiMappingStatusService;
            _mappingCertification = mappingCertification;
            _log.InfoFormat(StringResource.RegistrationStarted);
        }

        public void Process(string docUrl)
        {
            _log.InfoFormat(StringResource.RegReqReceived, docUrl);
            var apiMappingStatusInfo = _apiMappingStatusService.GetApiStatusInfo(docUrl) ?? _apiMappingStatusService.CreateApiStatusInfo(docUrl);
            try
            {
                Ensure.ThatStringIsNotNullOrEmpty(docUrl, nameof(docUrl));
                var client = new WebApiClient();
                if (_apiMappingStatusService.IsRecentlyProcessed(apiMappingStatusInfo))
                {
                    _log.Warn(
                        $"{docUrl} recently processed within the last {_apiMappingStatusService.RegistrationThreshold} minutes; therefore, the registration request will be ignored.");
                    return;
                }

                var swaggerText = client.Get(docUrl);

                //TODO: mapping certification will be activated once it passes the initial certification test.
                _log.Info($"Certification of {docUrl} started.");
                if (!_mappingCertification.IsAllMappingCertified(swaggerText))
                {
                    _log.Error($"Mappings of {docUrl} failed the certification and cannot be registered with IH");
                    return;
                }
                _log.Info($"{docUrl} passed the certification.");


                DeleteAllMappingsForCurrentDocUrl(docUrl);

                var mappings = _swaggerParser.GetAllApiMappings(swaggerText).ToList();

                SetGlobalTypeNames(mappings);

                InsertAllMappings(mappings, docUrl);
                apiMappingStatusInfo.LastRegistered = DateTime.Now;
                _apiMappingStatusService.UpdateApiStatusInfo(apiMappingStatusInfo);
                UpdateCustomMapPostOnly(mappings);
            }
            catch (Exception e)
            {
                _log.Error(string.Format(StringResource.RegReqFailed, docUrl), e);
            }
            _log.InfoFormat(StringResource.RegReqCompleted, docUrl);
        }

        public Task ProcessAsync(string body)
        {
            throw new NotImplementedException();
        }

        private void SetGlobalTypeNames(IEnumerable<GeneratedMapping> mappings)
        {
            var globalTypeNames = _globalLookupTypeReader.GetLookupNames().ToList();
            foreach (var fieldDefinition in mappings.SelectMany(mapping => mapping.Mapping.FieldDefinitions
                .Where(
                    fieldDefintion =>
                        globalTypeNames.Contains(fieldDefintion.Source, StringComparer.CurrentCultureIgnoreCase))
                .Where(fieldDefintion => string.IsNullOrWhiteSpace(fieldDefintion.GlobalLookupType))))
            {
                fieldDefinition.GlobalLookupType = fieldDefinition.Source;
            }
        }

        private void InsertAllMappings(IEnumerable<GeneratedMapping> mappings, string swaggerDocUrl)
        {
            foreach (var mapping in mappings)
            {
                mapping.MappingEndpoints = mapping.MappingEndpoints;
                mapping.DocUrl = swaggerDocUrl;
                mapping.HasHeader = null;

                if (string.IsNullOrEmpty(mapping.MappingDescription))
                {
                    mapping.MappingDescription = MappingDescription.GetMappingDescription(mapping.MappingName);
                }

                var fieldDefinitions = mapping.Mapping.FieldDefinitions.ToList();

                if (mapping.IsEndPointOtherThanPost())
                {
                    fieldDefinitions.Add(new MappingFieldDefinition
                    {
                        Source = ImportConstants.ActionColumnName,
                        Destination = ImportConstants.ActionFieldName,
                        Required = false,
                        Type = "string"
                    });
                }
              
                mapping.Mapping.FieldDefinitions = fieldDefinitions;
                var added = _serviceRegistrationRepository.CreateItemAsync(mapping).Result;
                _log.Info(
                    $"Mapping name: {mapping.MappingName} with endpoints: Post: {mapping.MappingEndpoints.Post} Put: {mapping.MappingEndpoints.Put} Delete: {mapping.MappingEndpoints.Delete} added.");
                _log.Debug(mapping);
            }
        }

        

        private void DeleteAllMappingsForCurrentDocUrl(string swaggerDocUrl)
        {
            var items = _serviceRegistrationRepository.GetItems(t => t.DocUrl.ToLower() == swaggerDocUrl.ToLower());
            foreach (var item in items)
            {
                try
                {
                    _log.Info(
                        $"Mapping name: {item.MappingName} with endpoints: Post: {item.MappingEndpoints?.Post} Put: {item.MappingEndpoints?.Put} Delete: {item.MappingEndpoints?.Delete} removed.");
                    var deleted = _serviceRegistrationRepository.DeleteItemAsync(item.Id).Result;
                }

                catch (Exception ex)
                {
                    _log.Error(
                        $"Unable to delete mapping for {item.MappingName} with endpoints: Post: {item.MappingEndpoints?.Post} Put: {item.MappingEndpoints?.Put} Delete: {item.MappingEndpoints?.Delete}.",
                        ex);
                }
                _log.Debug(item);
            }
        }

        private void UpdateCustomMapPostOnly(IEnumerable<GeneratedMapping> apiMappings)
        {
            var listOfInvalidCustomMaps = new List<GeneratedMapping>();
            var regMapsNameWithOnlyPosts =
                apiMappings.Where(
                    t =>
                        t.MappingEndpoints.Put == null && t.MappingEndpoints.Patch == null &&
                        t.MappingEndpoints.Delete == null).Select(t => t.MappingName);
            foreach (var mapName in regMapsNameWithOnlyPosts)
            {
                var customMapsWithOnlyPosts =
                    _serviceRegistrationRepository.GetItems(t => t.DocUrl == null && t.ObjectType == mapName);
                if(!customMapsWithOnlyPosts.Any()) continue;
                listOfInvalidCustomMaps.AddRange(customMapsWithOnlyPosts.Where(customMap => customMap.Mapping.FieldDefinitions.Any(t => t.Destination == ImportConstants.ActionFieldName)));
            }
            if (!listOfInvalidCustomMaps.Any()) return;
            foreach (var invalidCustomMap in listOfInvalidCustomMaps)
            {
                var map = invalidCustomMap.RemoveActionFieldFromMap();
                var updated = _serviceRegistrationRepository.UpdateItemAsync(map.Id, map);
                _log.Info($"Cutom map {map.MappingName} of object type {map.ObjectType} has been updated");
            }
        }            
    }
}
