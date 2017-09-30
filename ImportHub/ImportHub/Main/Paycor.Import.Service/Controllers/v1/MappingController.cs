using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Security.Principal;
using Swashbuckle.Swagger.Annotations;
using Paycor.Import.Security;
using Paycor.Import.Service.Shared;
using System.Threading.Tasks;
using Paycor.Import.Azure;
using Paycor.Import.FailedRecordFormatter;
// ReSharper disable All

namespace Paycor.Import.Service.Controllers.v1
{
    /// <summary>
    /// A controller for processing uploaded file.
    /// </summary>
    [Authorize]
    [RoutePrefix("importhub/v1")]
    public class MappingController : ApiController
    {
        #region Fields
        private readonly ILog _log;
        private readonly PaycorUserPrincipal _principal;
        private readonly IDocumentDbRepository<GeneratedMapping> _apiMappingRepository;
        private readonly IDocumentDbRepository<ClientMapping> _clientMappingRepository;
        private readonly IDocumentDbRepository<GlobalMapping> _globalMappingRepository;
        private readonly IDocumentDbRepository<UserMapping> _userMappingRepository;
        private readonly SecurityValidator _securityValidator;
        private readonly IExcelToCsv _excelToCsv;
        private readonly IXlsxRecordFormatter<string> _xlsxDataFormatter;
        private readonly IFileStorageProvider _fileStorageProvider;
        private readonly ITableStorageProvider _tableStorageProvider;
        private readonly IMapService _mapService;
        
        #endregion

        #region Public methods
        public MappingController(ILog log,
            IDocumentDbRepository<GeneratedMapping> apiMappingRepository,
            IDocumentDbRepository<ClientMapping> clientMappingRepository,
            IDocumentDbRepository<GlobalMapping> globalMappingRepository,
            IDocumentDbRepository<UserMapping> userMappingRepository,
            IExcelToCsv excelToCsv,
            IFileStorageProvider fileStorageProvider, IXlsxRecordFormatter<string> xlsxDataFormatter,
            ITableStorageProvider tableStorageProvider,
            IMapService mapService)
        {
            Ensure.ThatArgumentIsNotNull(apiMappingRepository, nameof(apiMappingRepository));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(fileStorageProvider, nameof(fileStorageProvider));
            Ensure.ThatArgumentIsNotNull(xlsxDataFormatter, nameof(xlsxDataFormatter));
            Ensure.ThatArgumentIsNotNull(tableStorageProvider, nameof(tableStorageProvider));
            Ensure.ThatArgumentIsNotNull(clientMappingRepository, nameof(clientMappingRepository));
            Ensure.ThatArgumentIsNotNull(globalMappingRepository, nameof(globalMappingRepository));
            Ensure.ThatArgumentIsNotNull(userMappingRepository, nameof(userMappingRepository));
            Ensure.ThatArgumentIsNotNull(mapService, nameof(mapService));


            _log = log;
            _principal = HttpContext.Current.User as PaycorUserPrincipal;
            _apiMappingRepository = apiMappingRepository;
            _clientMappingRepository = clientMappingRepository;
            _globalMappingRepository = globalMappingRepository;
            _userMappingRepository = userMappingRepository;
            _securityValidator = _securityValidator ?? (_securityValidator = new SecurityValidator(_log, _principal));
            _xlsxDataFormatter = xlsxDataFormatter;
            _excelToCsv = excelToCsv;
            _fileStorageProvider = fileStorageProvider;
            _tableStorageProvider = tableStorageProvider;
            _mapService = mapService;
        }


        /// <summary>
        /// Gets the specified mapping. This operation has an additional side effect of
        /// validating the returned mapping against all registered APIs to determine validity.
        /// </summary>
        /// <param name="id">The Id of the saved custom map.</param>
        /// <returns>GeneratedMapping</returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(ApiMapping))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("mappings/{id}")]
        public IHttpActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                var dictionary = new Dictionary<string, string> { ["id"] = "could not be found." };
                return this.HtmlResponse(HttpStatusCode.NotFound, "Mapping not found",
                    "The requested map could not be found.", dictionary);
            }
            var apiMapping = _apiMappingRepository.GetItem(e => e.Id == id);
            if (apiMapping != null)
            {
                apiMapping.UpdateGeneratedMappingName();
                return Ok(apiMapping);
            }

            var userMapping = _userMappingRepository.GetItem(e => e.Id == id);
            if (userMapping != null && CannotAccessUserMapping(userMapping))
            {
                return UnauthorizedAccessResponse();
            }

            var globalMapping = _globalMappingRepository.GetItem(e => e.Id == id);
            var clientMapping = _clientMappingRepository.GetItem(e => e.Id == id);
            int clientId;
            int.TryParse(clientMapping?.ClientId, out clientId);

            if (clientMapping == null && globalMapping == null && userMapping == null)
            {
                var dictionary = new Dictionary<string, string> { ["id"] = "could not be found." };
                return this.HtmlResponse(HttpStatusCode.NotFound, "Item not found",
                    "The requested item could not be found.", dictionary);
            }

            CommonSavedMapping mapping;
            if (clientMapping != null)
                mapping = clientMapping;
            else if (globalMapping != null)
            {
                mapping = globalMapping;
            }
            else
            {
                mapping = userMapping;
            }

            var endPoints = mapping.MappingEndpoints;
            var allMatchedMappings = GetAllMatchedMappings(endPoints, mapping.ObjectType);
            var matchedMappings = allMatchedMappings as GeneratedMapping[] ?? allMatchedMappings.ToArray();
            var commonSavedMapping = mapping;
            commonSavedMapping.UpdateGeneratedMappingName();
            if (!matchedMappings.Any())
            {
                commonSavedMapping.IsMappingValid = false;
                return Ok(commonSavedMapping);
            }
            var result = IsCustomMappingValid(matchedMappings, mapping);
            if (result) return Ok(commonSavedMapping);

            commonSavedMapping.IsMappingValid = false;
            return Ok(commonSavedMapping);
        }


        /// <summary>
        /// Converts the list of mapping. 
        /// </summary>
        /// <param name="mapinfo">Info for maps to be Converted. Valid Values for currentMapType and convertToMapType is Global,Client,User</param>
        /// <returns>GeneratedMapping</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("mappings/convertmaps")]
        [HttpPut]
        public async Task<IHttpActionResult> ConvertMap(ConvertMapInfo mapinfo)
        {
            try
            {
                if (!HasMapManagementPrivilege())
                {
                    return UnauthorizedAccessResponse();
                }

                var errorsList = await _mapService.ConvertMapsAsync(mapinfo);

                if (errorsList.Any())
                    return GetResult(errorsList, "Unable to convert the requested Mapping.");

                return Ok();

            }
            catch (Exception ex)
            {
                _log.Error("An Error occurred while attempting to convert a mapping.", ex);
                return this.ExceptionResponse(ex);
            }
        }

      
        /// <summary>
        /// Deletes the list of mapping. 
        /// </summary>
        /// <param name="mapinfo">Info for maps to be deleted. Valid Values for MapType is Global,Client,User</param>
        /// <returns>GeneratedMapping</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.ExpectationFailed, type: typeof(ErrorResponse))]
        [Route("mappings/delete")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(DeleteMapInfo mapinfo)
        {
            try
            {
                if (!HasMapManagementPrivilege())
                {
                    return UnauthorizedAccessResponse();
                }

                var errorsList = await _mapService.DeleteMapsAsync(mapinfo);

                if (errorsList.Any())
                    return GetResult(errorsList, "Unable to delete the requested Mapping.");

                return Ok();

            }
            catch (Exception ex)
            {
                _log.Error("An Error occurred while attempting to delete a mapping.", ex);
                return this.ExceptionResponse(ex);
            }
        }


        private IHttpActionResult GetResult(Dictionary<string,string> errorsList, string detail)
        {
            return this.HtmlResponse(HttpStatusCode.ExpectationFailed, "One or More Items Failed",
                detail, errorsList);
        }

        protected bool HasMapManagementPrivilege(int? clientId = null)
        {
            return _securityValidator.IsAuthorizedForMapManagementPrivilege(clientId);
        }

        /// <summary>
        /// Gets all mappings for the current user.
        /// </summary>
        /// <param name="registeredMaps">True or False based on whether registered maps are needed or not</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof (IEnumerable<GeneratedMapping>))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [Route("mappings")]
        public IHttpActionResult Get(bool registeredMaps = false)
        {
            var mappings = new List<ApiMapping>();
            AddApiMappings(mappings);

            if (registeredMaps)
            {
                mappings.UpdateGeneratedMappingName();
                return Ok(mappings.OrderBy(t=>t.MappingName));
            }

            if (HasGlobalMappingPrivilege())
            {
                AddGlobalMappings(mappings);
            }

            if (HasClientMappingPrivilege())
            {
                AddClientMappings(mappings);
            }

            AddUserMappings(mappings);

            mappings.UpdateGeneratedMappingName();
            return Ok(mappings.OrderBy(t=>t.MappingName));
        }

        private void AddUserMappings(List<ApiMapping> mappings)
        {
            var userMappings = _userMappingRepository.GetItemsFromSystemType(typeof(UserMapping).ToString());
            mappings.AddRange(userMappings);
        }

        private void AddClientMappings(List<ApiMapping> mappings)
        {
            var rls = _principal.GetRowLevelSecurity(PrivilegeConstants.ClientMapPrivilegeId);
            var clients = rls.GetListofPrivilegeClients();

            var clientMappings = _clientMappingRepository.GetItemsFromSystemType(typeof(ClientMapping).ToString());

            if (!rls.DoesHaveAccessToAllClients())
            {
                clientMappings = clientMappings.Where(t => clients.Contains(t.ClientId));
            }
            mappings.AddRange(clientMappings);
        }

        private void AddApiMappings(List<ApiMapping> mappings)
        {
            var apiMappings = _apiMappingRepository.GetItemsFromSystemType(typeof(GeneratedMapping).ToString());
            mappings.AddRange(apiMappings);
        }

        private void AddGlobalMappings(List<ApiMapping> mappings)
        {
            var globalMappings = _globalMappingRepository.GetItemsFromSystemType(typeof(GlobalMapping).ToString());
            mappings.AddRange(globalMappings);
        }

        /// <summary>
        /// Gets the template for the mapping selected in Import Hub, either in csv or xlsx format.
        /// The file will be named in the below format
        /// MappingName_Template.csv or MappingName_Template.xlsx
        /// </summary>
        /// <param name="id">The mapping id of the map.</param>
        /// <param name="formatType">csv or xlsx. Valid Values are .csv and .xlsx</param>
        /// that will be returned, otherwise the current server time will be appended to the filename.
        /// <returns>returns template of selected map.</returns>
        [SwaggerResponse(HttpStatusCode.OK, description: "the template of selected map", type: typeof(IEnumerable<byte>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [HttpGet]
        [Route("mappings/{id}/template")]
        public async Task<HttpResponseMessage> GetTemplate(string id, string formatType = ImportConstants.XlsxFileExtension)
        {
            try
            {
                byte[] fileBytes;
                const string fileName = "Template";
                if (string.IsNullOrWhiteSpace(id))
                {
                    return this.ReturnHttpNotFound(ExceptionMessages.FailedCsvInvalidLookup);
                }
                var mapping = ((_apiMappingRepository.GetItem(e => e.Id == id) ??
                                        (ApiMapping) _clientMappingRepository.GetItem(e => e.Id == id)) ??
                                       _globalMappingRepository.GetItem(e => e.Id == id)) ??
                                      _userMappingRepository.GetItem(e => e.Id == id);

                if (mapping == null)
                {
                    return this.ReturnHttpNotFound("Item not found");
                }
             
                var formattedFileName = Path.GetFileNameWithoutExtension(fileName).FormatFileName(formatType, mapping.MappingName);
                var lookupFileNameInStorage = $"{mapping.MappingName}{formatType}";
                var fileBytesFromStorage = await _fileStorageProvider.GetFileFromFileStorage(lookupFileNameInStorage, mapping.ObjectType).ConfigureAwait(false);

                if (fileBytesFromStorage != null)
                {
                    if (fileBytesFromStorage.Length != 0)
                    {
                        _log.Debug($"Template {formattedFileName} downloaded from IH file share.");
                        fileBytes = GetCsvOrExcelBytesForMappingTemplate(fileBytesFromStorage, formatType);
                        return this.ReturnHttpOk(fileBytes, formattedFileName, formatType.GetMediaTypeHeaderValue());
                    }
                }

                var listOfHeaders =
                    mapping.Mapping.GetHeaderFieldsFromMappingFieldsWithActionOnTop()
                    .ToList();

                var excelFileBytes = _xlsxDataFormatter.GenerateXlsxData(listOfHeaders);
                fileBytes = GetCsvOrExcelBytesForMappingTemplate(excelFileBytes, formatType);
                return fileBytes != null ? this.ReturnHttpOk(fileBytes, formattedFileName, formatType.GetMediaTypeHeaderValue()) : this.ReturnHttpNotFound(ExceptionMessages.FailedCsvNoFileFoundInBlob);
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(FilesController)}:{nameof(GetTemplate)} action", ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                                                ExceptionMessages.FailedCsvNotFound));
            }            
        }

        /// <summary>
        /// Returns the map activity of what ever happend to the map.
        /// </summary>
        /// <param name="mappingName">The map name</param>
        /// <returns>returns all the audit info for specified map.</returns>
        [SwaggerResponse(HttpStatusCode.OK, description: "all map activity", type: typeof(IEnumerable<ApiMappingAudit>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("mappings/audit/mapname")]
        public IHttpActionResult GetMapAudit(string mappingName)
        {
            var mappingAudits = _tableStorageProvider.GetItems(mappingName);
            var apiMappingAudits = mappingAudits as ApiMappingAudit[] ?? mappingAudits.ToArray();
            if (apiMappingAudits.Any())
            {
                var displayAudit =
                    apiMappingAudits.Select(
                        t => new {t.MappingName, t.MappingId, t.Action, t.CurrentDateTime, t.UserName, t.Reason}); 
                return Ok(displayAudit);
            }
            return this.HtmlResponse(HttpStatusCode.NotFound, "No audit",
                "No mappings audit found.", null);
        }
        #endregion

        #region Private methods
        private static bool IsCustomMappingValid(IEnumerable<GeneratedMapping> allMatchedMappings, ApiMapping mapping)
        {
            //In case of expanded col custom map we need only the distinct destinations as they are repeated.
            var customMapDestinations = mapping.Mapping.GetAllDestinationFields().Select(t => t.RemoveIndexValueFromString());
            var mapDestinations = customMapDestinations.Distinct().ToArray();
            foreach (var matchedMapping in allMatchedMappings)
            {
                var matchedMapDestination = matchedMapping.Mapping.GetAllDestinationFields();
                if (mapDestinations.ExistIn(matchedMapDestination))
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<GeneratedMapping> GetAllMatchedMappings(MappingEndpoints endPoints, string objectType)
        {
            var allMatchedMappings = _apiMappingRepository.GetItems(
                                       e => (e.MappingEndpoints.Post == endPoints.Post) && (e.MappingEndpoints.Put == endPoints.Put)
                                       && (e.MappingEndpoints.Delete == endPoints.Delete) && (e.MappingEndpoints.Patch == endPoints.Patch));
            var items = allMatchedMappings.Where(e => e.DocUrl != null && e.ObjectType == objectType);
               
            return items;
        }


        private bool HasGlobalMappingPrivilege(int? clientId = null)
        {
            return _securityValidator.IsAuthorizedForGlobalMapPrivilege(clientId);
        }

        private bool HasClientMappingPrivilege(int? clientId = null)
        {
            return _securityValidator.IsAuthorizedForClientMapPrivilege(clientId);
        }

        private bool CannotAccessUserMapping(ApiMapping mapping)
        {
            return (mapping is UserMapping && HasGlobalMappingPrivilege());
        }

        private IHttpActionResult UnauthorizedAccessResponse()
        {
            return this.HtmlResponse(HttpStatusCode.Unauthorized, "Access denied",
                "You are not authorized to do this operation.", null);
        }

        private byte[] GetCsvOrExcelBytesForMappingTemplate(byte[] excelBytes, string formatType)
        {
            return formatType != ImportConstants.XlsxFileExtension ? _excelToCsv.Convert(excelBytes) : excelBytes;
        }

        #endregion
    }
}