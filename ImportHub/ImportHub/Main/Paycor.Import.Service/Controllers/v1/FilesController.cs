using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.FileDataExtracter.LegacyShim;
using Paycor.Import.FileType;
using Paycor.Import.ImportHistory;
using Paycor.Import.Mapping;
using Paycor.Import.Security;
using Paycor.Import.Service.FileType;
using Paycor.Import.Service.Shared;
using Paycor.Import.Status;
using Paycor.Import.Status.Azure;
using Paycor.Import.Validator;
using Paycor.Security.Principal;
using Swashbuckle.Swagger.Annotations;
using System.Configuration;

namespace Paycor.Import.Service.Controllers.v1
{
    /// <summary>
    /// A controller for processing uploaded file.
    /// </summary>
    [Authorize]
    [RoutePrefix("importhub/v1")]
    public class FilesController : ApiController
    {
        private readonly string _blobConnectionString = ConfigurationManager.AppSettings["BlobStorageConnection"];
        private readonly ILog _log;
        private readonly IFileTypeHandlerFactory _factory;
        private readonly PaycorUserPrincipal _principal;
        private readonly IImportHistoryService _importHistoryService;
        private readonly IExcelToCsv _excelToCsv;
        private readonly IValidator<ApiMapping> _validator;
        private readonly IProvideSheetData _provideSheetData;
        private SecurityValidator _securityValidator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="log">log for logging purpose.</param>
        /// <param name="fieldMapper">To get or find the APIMapping and Mapping Definations.</param>
        /// <param name="importHistoryService">Keeps track of import history, gets import history by user and returns failed records based on each file imported.</param>
        /// <param name="excelToCsv">Converts Xlsx to CSV bytes</param>
        /// <param name="validator">Validates Mapping Definition</param>
        /// <param name="provideSheetData">sheet data provider</param>
        public FilesController(ILog log, IFieldMapper fieldMapper, IImportHistoryService importHistoryService,
            IExcelToCsv excelToCsv, IValidator<ApiMapping> validator, IProvideSheetData provideSheetData)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(fieldMapper, nameof(fieldMapper));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            Ensure.ThatArgumentIsNotNull(excelToCsv, nameof(excelToCsv));
            Ensure.ThatArgumentIsNotNull(validator, nameof(validator));
            Ensure.ThatArgumentIsNotNull(provideSheetData, nameof(provideSheetData));

            _log = log;
            _importHistoryService = importHistoryService;
            _excelToCsv = excelToCsv;
            _factory = new ImportHubFileTypeHandlerFactory(_log, _importHistoryService, fieldMapper);
            _principal = HttpContext.Current.User as PaycorUserPrincipal;
            _validator = validator;
            _provideSheetData = provideSheetData;
        }

        #region Public Methods
        /// <summary>
        /// Uploads a file along with the selected mappings into the import hub as multipart form data. Note: This post
        /// only supports data sent as MIME multipart content. 
        /// </summary>
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [HttpPost]
        [Route("files")]
        public async Task<IHttpActionResult> Post()
        {
            NewRelic.Api.Agent.NewRelic.IgnoreApdex();

            if (!Request.Content.IsMimeMultipartContent())
            {
                return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType, Request);
            }

            try
            {
                var principal = HttpContext.Current.User as PaycorUserPrincipal;
                var filePath = HttpContext.Current.Server.MapPath("~/app_data");
                Ensure.ThatArgumentIsNotNull(filePath, nameof(filePath));

                var provider = new MultipartFormDataStreamProvider(filePath);
                await Request.Content.ReadAsMultipartAsync(provider);

                var mappingString = provider.FormData["mapping"];

                FileTypeInfo info;
                if (
                    Enum.GetNames(typeof(ImportConstants.EeImportMappingEnum))
                        .Select(t => t.ToLower())
                        .Contains(mappingString.ToLower()))
                {
                    _log.Info("Import type set to legacy EE Import.");
                    info = new EeFileTypeInfo
                    {
                        FileType = FileTypeEnum.EmployeeImport,
                        MappingValue = mappingString
                    };
                }
                else
                {
                    ApiMapping[] mappings = null;
                    try
                    {
                        mappings = JsonConvert.DeserializeObject<ApiMapping[]>(mappingString);
                    }
                    catch (JsonSerializationException)
                    {
                        // eat it.
                    }
                    if (mappings != null)
                    {
                        if (mappings.Length < 1)
                        {
                            var response = new Dictionary<string, string>
                            {
                                ["No mappings"] = "No valid mappings were supplied with the uploaded file."
                            };
                            return this.ValidationResponse(response);
                        }
                        foreach (var map in mappings)
                        {
                            var mappingErrorResponse = ValidateMappingDefinition(map);
                            if (mappingErrorResponse.Any())
                            {
                                return this.ValidationResponse(mappingErrorResponse);
                            }
                        }

                        info = new MappedFileTypeInfo
                        {
                            FileType = FileTypeEnum.MappedFileImport,
                            Mappings = mappings,
                            IsMultiSheet = true
                        };

                        var multisheetPartFileData = provider.FileData.FirstOrDefault();
                        if (multisheetPartFileData != null)
                        {
                            using (var stream = new FileStream(multisheetPartFileData.LocalFileName, FileMode.Open))
                            {
                                var sheetNames =
                                    _provideSheetData.GetAllSheetNames(stream).Select(t => t.ToLower()).ToList();
                                var mappingNames = mappings.Select(t => t.MappingName.ToLower()).ToList();
                                var invalidSheetNames = mappingNames.GetNotContainedValues(sheetNames);
                                if (invalidSheetNames.Count > 0)
                                {
                                    var notContainedSheetNames = invalidSheetNames.ConcatListOfString(",");
                                    var validationResponse = new Dictionary<string, string>
                                    {
                                        ["No sheet mapping"] =
                                        $"The following work sheets do not have an associated mapping: {notContainedSheetNames}. Please ensure that a map exists for these sheets."
                                    };
                                    return this.ValidationResponse(validationResponse);
                                }
                            }
                        }
                    }
                    else
                    {
                        var mapping = JsonConvert.DeserializeObject<ApiMapping>(mappingString);
                        _log.Info("Import type set to Mapped file import");
                        _log.Debug($"Mapping is {mapping}");

                        var mappingErrorResponse = ValidateMappingDefinition(mapping);
                        if (mappingErrorResponse.Any())
                        {
                            return this.ValidationResponse(mappingErrorResponse);
                        }

                        info = new MappedFileTypeInfo
                        {
                            FileType = FileTypeEnum.MappedFileImport,
                            Mappings = new[] {mapping},
                            IsMultiSheet = false
                        };
                    }
                }

                var handler = _factory.GetFileTypeHandler(info.FileType);
                var multipartFileData = provider.FileData.FirstOrDefault();

                if (multipartFileData == null)
                {
                    return BadRequest(ExceptionMessages.FileUploadNoFilesUploaded);
                }
                var statusMessageId = handler.HandleFile(info, multipartFileData, principal);
                LogicalThreadContext.Properties[ImportConstants.Transaction] = statusMessageId;
                _log.Info("File queued for import.");
                File.Delete(multipartFileData.LocalFileName);
                return CreatedAtRoute(ImportConstants.RouteFilesGet, new {id = statusMessageId}, statusMessageId);
            }
            catch (Exception ex)
            {
                _log.Error(ExceptionMessages.ErrorOnFileUpload, ex);
                return this.ExceptionResponse(ex);
            }
        }

        /// <summary>
        /// Provides a means to test validation of the mapping which is provided as part of a multipart post 
        /// on the files endpoint. Note: This endpoint should not be called by any applications.
        /// </summary>
        /// <param name="mapping">the mapping to validate</param>
        /// <returns>OK, if validation passes; otherwise, internal server error.</returns>
        [HttpPost]
        [Route("files/test", Name = "testharness")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult Post(GeneratedMapping mapping)
        {
            var mappingErrorResponse = ValidateMappingDefinition(mapping);
            return mappingErrorResponse.Any() ? this.ValidationResponse(mappingErrorResponse) : Ok();
        }

        /// <summary>
        /// Do not use this route. It exists to document the arguments of the multipart post for files.
        /// </summary>
        /// <param name="fileTypeMappingInfo">the type of file uploaded and its associated mapping selected by the user in order to call the selected mapping API.</param>
        /// <param name="file">the filestream to upload to the server.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("files_doc_ref_only")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult Post(FileTypeInfo fileTypeMappingInfo, HttpPostedFile file)
        {
            return
                BadRequest(
                    "Do not use this Post. This route exist solely to document the arguments of the multipart post for files.");
        }

        /// <summary>
        /// Gets the import status of the imported file.
        /// </summary>
        /// <param name="id">The Import Id for the import file.</param>
        /// <param name="statusDetails">Indicates if statusdetails need to be removed from the importstatus json</param>
        /// <returns>Returns the status message.</returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(MappedFileImportStatusMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [Route("files/{id}", Name = ImportConstants.RouteFilesGet)]
        public IHttpActionResult Get(string id, bool statusDetails = true)
        {
            try
            {
                var statusMessage = GetImportStatusMessage(id);
                if (IsValidUser(statusMessage))
                {
                    return FormatResultBasedOnImportType(id, statusDetails, statusMessage);
                }
                _log.Error(
                    $"Unauthorized {_principal.UserName} attempted to call {nameof(FilesController)}:{nameof(Get)} action");
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                _log.Info($"Could not find status message with id:{id}", ex);
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(FilesController)}:{nameof(Get)} action", ex);
                return this.ExceptionResponse(ex);
            }
        }

        /// <summary>
        /// Gets the failed rows for the file imported in Import Hub, either in csv or xlsx format.
        /// The file will be named in the below format
        /// ImportType_UploadedFileName_DateTime.csv or ImportType_UploadedFileName_DateTime.xlsx
        /// </summary>
        /// <param name="id">The import id of the file.</param>        
        /// <param name="formatType">csv or xlsx. Valid Values are .csv and .xlsx</param>
        /// <param name="dateTime">The current date time, if passed in will be appended to the FileName 
        /// that will be returned, otherwise the current server time will be appended to the filename.
        /// </param>
        /// <returns>returns all the failed rows.</returns>
        [SwaggerResponse(HttpStatusCode.OK, description: "the failed rows in CSV format",
             type: typeof(IEnumerable<byte>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [SwaggerResponse(HttpStatusCode.ServiceUnavailable)]
        [HttpGet]
        [Route("files/{id}/failedrows", Name = "failedrows.get")]
        public async Task<HttpResponseMessage> GetFailedRows(string id,
            string formatType = ImportConstants.CsvFileExtension, string dateTime = null)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(id))
                {
                    return this.ReturnHttpNotFound(ExceptionMessages.FailedCsvInvalidLookup);
                }
                var statusMessage = GetImportStatusMessage(id);

                if (!IsValidUser(statusMessage))
                {
                    _log.Error(
                        $"UnAuthorized {_principal.UserName} attempted to call {nameof(FilesController)}:{nameof(GetFailedRows)} action");
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }

                if (HasFailedRows(id))
                {
                    var fileBytes = await GetCsvOrExcelBytes(formatType, id);
                    var clientId = string.Empty;

                    if (statusMessage.ClientId != null)
                    {
                        clientId =   statusMessage.ClientId.Split(';').FirstOrDefault();
                    }

                    var formattedFileName = Path.GetFileNameWithoutExtension(statusMessage.FileName)
                                                        .FormatFileName(clientId, formatType, statusMessage.ImportType,
                                                                                        dateTime ?? DateTime.Now.ToString("yyyyMMddHHmm"));
                    return fileBytes != null
                        ? this.ReturnHttpOk(fileBytes, formattedFileName, GetMediaTypeHeaderValue(formatType))
                        : this.ReturnHttpNotFound(ExceptionMessages.FailedCsvNoFileFoundInBlob);
                }
                _log.Warn("There are no failed rows for the specified import");
                return this.ReturnHttpBadRequest("There are no failed rows for the specified import");
            }
            catch (StorageException ex)
            {
                _log.Error($"An Error Occurred in {nameof(FilesController)}:{nameof(GetFailedRows)} action", ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                    ExceptionMessages.FailedCsvNotFound));
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(FilesController)}:{nameof(GetFailedRows)} action", ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    ExceptionMessages.FailedCsvNotFound));
            }
        }

        /// <summary>
        /// Cancels an import in progress. The transaction to cancel must still be in progress. If not,
        /// a bad request response will be returned.
        /// </summary>
        /// <param name="id">the transaction id of the import to cancel</param>
        [HttpDelete]
        [Route("files/{id}")]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type:typeof(ErrorResponse))]
        public IHttpActionResult Delete(string id)
        {
            _log.Info($"A cancellation request has been received for import transaction: {id}.");
            try
            {

                var message = _importHistoryService.GetImportHistory(id);
                if (message.ImportHistoryStatus == ImportHistoryStatusEnum.Completed)
                {
                    _log.Warn(
                        $"The cancellation request could not be processed for import transaction {id} because it has already been completed.");
                    var validationResponse = new Dictionary<string, string>
                    {
                        ["Not supported"] = "Cancel is not supported for an import that has already been completed."
                    };
                    return this.ValidationResponse(validationResponse);
                }

                var handler = _factory.GetFileTypeHandler(GetFileType(message.FileType, message.FileName, message.ImportType));
                var user = SecurityValidator.GetUser();
                handler.Cancel(user, id, message.ImportDate);
                return Ok();
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(HistoryController)}:{nameof(Delete)} action", ex);
                return this.ExceptionResponse(ex);
            }
        }
        #endregion

        #region Private Methods
        private bool IsValidUser(ImportStatusMessage statusMessage)
        {
            //PAYCOR Users can see any result other users can only see thier own.
            return statusMessage.User == _principal.UserKey.ToString() || _principal.IsPaycor;
        }

        private ImportStatusMessage GetImportStatusMessage(string id)
        {
            var storageProvider = new BlobStatusStorageProvider(_blobConnectionString, ContainerNames.ImportStatus);
            var statusLogger = new ImportStatus<ImportStatusMessage>().GetStatusEngine(id, ContainerNames.ImportStatus,
                storageProvider);

            var message = statusLogger.RetrieveMessage();
            return message;
        }

        private IHttpActionResult FormatResultBasedOnImportType(string id, bool statusDetails,
            ImportStatusMessage message)
        {
            if (string.IsNullOrEmpty(message.ImportType))
                return Ok(message);

            var handler = _factory.GetFileTypeHandler(GetFileType(message.FileType, message.FileName, message.ImportType));

            var resultMessage = handler.SetResultsLink(id, _principal);
            if (resultMessage == null)
            {
                return NotFound();
            }

            if (statusDetails)
            {
                return Ok(resultMessage);
            }
            var simpleResultMessage = handler.ModifyMessage(resultMessage);
            return Ok(simpleResultMessage);
        }

        private bool HasFailedRows(string id)
        {
            var statusResponse = Get(id);
            var result = statusResponse as OkNegotiatedContentResult<ImportStatusMessage>;
            return result != null && result.Content.FailRecordsCount > 0;
        }

        private async Task<byte[]> GetCsvOrExcelBytes(string formatType, string id)
        {
            var excelBytes = await _importHistoryService.GetFailedRecordsFile(id).ConfigureAwait(false);

            return formatType != ImportConstants.XlsxFileExtension ? _excelToCsv.Convert(excelBytes) : excelBytes;
        }

        private static string GetMediaTypeHeaderValue(string formatType)
        {
            return formatType != ImportConstants.XlsxFileExtension
                ? "text/csv"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        private static FileTypeEnum GetFileType(string fileTypeString, string fileName, string importType)
        {
            if (!string.IsNullOrEmpty(fileTypeString))
            {
                FileTypeEnum fileType;

                Enum.TryParse(fileTypeString, true, out fileType);
                if (fileType == FileTypeEnum.Unrecognized) throw new Exception("File type is unrecognized.");
                return fileType;
            }

            // THIS IS A HACK TO OVERCOME A PREVIOUS HACK!
            // Originally, the code was written to determine file type based on the ImportType being "Employee".
            // Unforutnately, it is possible for a Mapfile File Import to have that same ImportType. The above code
            // was written to detect file type from a file type property which was added at the point of correction; 
            // however, the property will be empty for all imports done prior to the new property so the following code
            // will need to discern the old EE import from both the filename and the import type.
            if (fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) &&
                (importType.Trim().Equals(ImportConstants.Employee, StringComparison.OrdinalIgnoreCase) || 
                importType.Trim().Equals("Employee Import", StringComparison.OrdinalIgnoreCase)))
            {
                return FileTypeEnum.EmployeeImport;
            }
            return FileTypeEnum.MappedFileImport;
        }

        private IDictionary<string, string> ValidateMappingDefinition(ApiMapping mapping)
        {
            var errorResponse = new Dictionary<string, string>();
            string errorMessage;

            var isValidMapping = _validator.Validate(mapping, out errorMessage);
            if (!isValidMapping)
            {
                errorResponse = (Dictionary<string, string>) ErrorResponse("Mapping", $"Invalid Mapping:{errorMessage}");
            }
            return errorResponse;
        }

        private IDictionary<string, string> ErrorResponse(string key, string value)
        {
            _log.Debug($"{key}: {value}");

            var mappingDefInvalid = new Dictionary<string, string> {[key] = value};
            return mappingDefInvalid;
        }

        private SecurityValidator SecurityValidator => _securityValidator ?? (_securityValidator = new SecurityValidator(_log, _principal));

        #endregion
    }
}
