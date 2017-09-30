using log4net;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using Paycor.Import.Security;
using Paycor.Import.Validator;
using Paycor.Security.Principal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
// ReSharper disable All

namespace Paycor.Import.Service.Controllers.Unversioned
{
    public abstract class BaseSavedMappingController<T> : ApiController where T : CommonSavedMapping, new()
    {
        private readonly ILog _log;
        private readonly PaycorUserPrincipal _principal;
        private readonly SecurityValidator _securityValidator;
        private readonly IDocumentDbRepository<T> _repository;
        private readonly ITableStorageProvider _tableStorageProvider;
        private readonly IValidator<T> _validator;

        protected BaseSavedMappingController(ILog log, IDocumentDbRepository<T> repository, ITableStorageProvider tableStorageProvider, IValidator<T> validator)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(repository, nameof(repository));
            Ensure.ThatArgumentIsNotNull(tableStorageProvider, nameof(tableStorageProvider));
            Ensure.ThatArgumentIsNotNull(validator, nameof(validator));

            _log = log;
            _repository = repository;
            _tableStorageProvider = tableStorageProvider;
            _validator = validator;
            _principal = HttpContext.Current.User as PaycorUserPrincipal;
            _securityValidator = _securityValidator ?? (_securityValidator = new SecurityValidator(_log, _principal));
        }

        #region Public
        public virtual IHttpActionResult Get(string id)
        {
            var mapping = Repository.GetItem(map => map.Id == id);
            if (mapping == null)
            {
                var srcNotFound = new Dictionary<string, string>
                {
                    ["Id"] = "item not found"
                };
                return this.HtmlResponse(HttpStatusCode.NotFound, "Not Found", "The requested item was not found.", srcNotFound);
            }

            mapping.UpdateGeneratedMappingName();
            return Ok(mapping);
        }

        public virtual IHttpActionResult Get()
        {
            var mappings = Repository.GetItems();
            mappings.UpdateGeneratedMappingName();
            return Ok(mappings);
        }
        
        public virtual async Task<IHttpActionResult> Post(T mapping)
        {
            try
            {
                mapping.IsMappingValid = true;
                mapping.Id = null;

                mapping.Mapping.FieldDefinitions.RemoveWhiteSpaceFromFieldDefintionSource();

                var mappingErrorResponse = ValidateMappingDefinition(mapping);
                if (mappingErrorResponse.Any())
                {
                    return this.ValidationResponse(mappingErrorResponse);
                }

                if (mapping.IsMappingSourceEmptyOrNull())
                {
                    var response = new Dictionary<string, string> { ["Source"] = "Source cannot be null or empty." };
                    return this.ValidationResponse(response);
                }

                var document = await SaveMappingAsync(mapping);
                var customMap = JsonConvert.DeserializeObject<T>(document.ToString());
                _tableStorageProvider.Insert(CreateMapAudit(customMap, HtmlVerb.Post.GetActionFromVerb()));
                return Created(ImportConstants.RouteFilesGet, customMap);
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
            catch (Exception ex)
            {
                _log.Error("An error occurred while attempting to create a mapping.", ex);
                return this.ExceptionResponse(ex);
            }
        }

        public virtual IHttpActionResult Put(string id, T mapping)
        {
            if (mapping == null)
            {
                var mappingNotNull = new Dictionary<string, string> { ["Mapping"] = "cannot be null or empty" };
                return this.ValidationResponse(mappingNotNull);
            }
            if (id != mapping.Id)
            {
                var dictionary = new Dictionary<string, string> { ["id"] = "changes are not allowed for this property" };
                return this.ValidationResponse(dictionary);
            }
            if (!mapping?.IsMappingValid ?? false)
            {
                mapping.IsMappingValid = true;
            }
            if (string.IsNullOrWhiteSpace(mapping.MappingName))
            {
                var mappingNameNotNull = new Dictionary<string, string> { ["MappingName"] = "cannot be null or empty" };
                return this.ValidationResponse(mappingNameNotNull);
            }
            if (IsDuplicateMapping(mapping.MappingName.Trim(), true))
            {
                var mappingNameExists = new Dictionary<string, string> { ["MappingName"] = "already exists. Please select a unique mapping name." };
                return this.ValidationResponse(mappingNameExists);
            }

            mapping.Mapping.FieldDefinitions.RemoveWhiteSpaceFromFieldDefintionSource();

            var mappingErrorResponse = ValidateMappingDefinition(mapping);
            if (mappingErrorResponse.Any())
            {
                return this.ValidationResponse(mappingErrorResponse);
            }
            if (mapping.IsMappingSourceEmptyOrNull())
            {
                var response = new Dictionary<string, string> { ["Source"] = "Source cannot be null or empty." };
                return this.ValidationResponse(response);
            }            
            if (mapping.SystemType != typeof(T).FullName)
            {
                var response = new Dictionary<string, string>
                {
                    ["Update failed"] = $"Cannot update the map of type {mapping.SystemType.GetLastWordFromSentence(".")} to {typeof(T).ToString().GetLastWordFromSentence(".")}"
                };
                return this.ValidationResponse(response);
            }

            try
            {
                var updated = _repository.UpdateItemAsync(id, mapping).ConfigureAwait(false);
                if (updated.GetAwaiter().IsCompleted)
                {
                    var dictionary = new Dictionary<string, string> { ["id"] = "not found in mappings" };
                    return this.HtmlResponse(HttpStatusCode.NotFound, "Not Found", "The requested item was not found.", dictionary);
                }
                _tableStorageProvider.Insert(CreateMapAudit(mapping, HtmlVerb.Put.GetActionFromVerb()));
            }
            catch (Exception ex)
            {
                _log.Error("An error occurred while attempting to update a mapping.", ex);
                return this.ExceptionResponse(ex);
            }
            return Ok("Mapping updated");
        }

        public virtual IHttpActionResult Delete(string id)
        {
            var detail = new Dictionary<string, string> { ["id"] = "could not be found." };
            var notFoundResult = this.HtmlResponse(HttpStatusCode.Gone, "Mapping gone",
                "The requested mapping is missing or has already been deleted.", detail);

            var mapping = _repository.GetItem(e => e.Id == id);
            if (mapping == null)
            {
                return notFoundResult;
            }
            try
            {
                var deleted = _repository.DeleteItemAsync(id).ConfigureAwait(false);
                _tableStorageProvider.Insert(CreateMapAudit(mapping, HtmlVerb.Delete.GetActionFromVerb()));
                return deleted.GetAwaiter().IsCompleted ? notFoundResult : Ok();
            }
            catch (Exception ex)
            {
                _log.Error("An Error occurred while attempting to delete a mapping.", ex);
                return this.ExceptionResponse(ex);
            }
        }

        //will be deleted once deployed to QSB.
        public virtual IHttpActionResult DeleteAll()
        {
            var detail = new Dictionary<string, string> { ["Maps"] = "could not be found." };
            var notFoundResult = this.HtmlResponse(HttpStatusCode.Gone, "Mapping gone",
               "The requested mapping is missing or has already been deleted.", detail);
            try
            {
                var allMaps = _repository.GetItems();
                if (!allMaps.Any())
                    return notFoundResult;

                foreach (var map in allMaps)
                {
                    var deleted = _repository.DeleteItemAsync(map.Id);
                    if(!deleted.IsCompleted)
                        _log.Warn($"An Error occurred while attempting to delete a mapping {map.MappingName}");
                }
                return Ok();
            }
            catch (Exception ex)
            {

                _log.Error($"An Error occurred while attempting to delete a mapping.", ex);
                return this.ExceptionResponse(ex);
            }
            
        }
        #endregion

        #region Protected
        protected PaycorUserPrincipal Principal => _principal;
        protected IDocumentDbRepository<T> Repository => _repository;
        protected SecurityValidator SecurityValidator => _securityValidator;

        protected bool HasGlobalMappingPrivilege(int? clientId = null)
        {
            return SecurityValidator.IsAuthorizedForGlobalMapPrivilege(clientId);
        }

        protected bool HasClientMappingPrivilege(int? clientId = null)
        {
            return SecurityValidator.IsAuthorizedForClientMapPrivilege(clientId);
        }

        protected bool HasMapManagementPrivilege(int? clientId = null)
        {
            return _securityValidator.IsAuthorizedForMapManagementPrivilege(clientId);
        }

        protected IHttpActionResult UnauthorizedAccessResponse()
        {
            return this.HtmlResponse(HttpStatusCode.Unauthorized, "Access denied",
                "You are not authorized to modify or remove this mapping.", null);
        }
        #endregion

        #region Private
        private async Task<Document> SaveMappingAsync(T mapping)
        {
            mapping.MappingName = mapping?.MappingName?.Trim();
            var addedDocument = await _repository.CreateItemAsync(mapping);
            if (addedDocument == null)
            {
                throw new Exception("An error occurred while saving the mapping.");
            }
            _log.InfoFormat($"Mapping name: {mapping.MappingName} added.");
            _log.Debug(mapping);

            return addedDocument;
        }

        private bool IsDuplicateMapping(string mappingName, bool isUpdate = false)
        {
            List<string> allMapNames;
            if (isUpdate)
            {
                allMapNames = _repository.GetItems().Select(t => t.MappingName).ToList();
                allMapNames.Remove(mappingName);
            }
            else
            {
                allMapNames = _repository.GetItems().Select(t => t.MappingName).ToList();
            }
            return allMapNames.Any(mapName => mapName.RemoveWhiteSpaces().Equals(mappingName.RemoveWhiteSpaces(), StringComparison.CurrentCultureIgnoreCase));
        }

        private IDictionary<string, string> ValidateMappingDefinition(T mapping)
        {
            var errorResponse = new Dictionary<string, string>();
            string errorMessage;

            var isValidMapping = _validator.Validate(mapping, out errorMessage);
            if (!isValidMapping)
            {
                errorResponse = (Dictionary<string, string>)ErrorResponse("Mapping", $"Invalid Mapping:{errorMessage}");
            }
            return errorResponse;
        }

        private IDictionary<string, string> ErrorResponse(string key, string value)
        {
            _log.Debug($"{key}: {value}");

            var mappingDefInvalid = new Dictionary<string, string> { [key] = value };
            return mappingDefInvalid;
        }

        private ApiMappingAudit CreateMapAudit(T mapping, string action)
        {
            var dt = DateTime.Now;
            var dateTime =
                $"{dt:d-M-yyyy HH:mm:ss}";
            var mapAudit = new ApiMappingAudit(mapping.MappingName.ToLower(),
                dateTime)
            {
                Action = action,
                MappingId = mapping.Id,
                Reason = string.Empty, //will be changed.
                UserName = Principal.UserName,
                MappingName = mapping.MappingName,
                CurrentDateTime = dateTime
            };

            return mapAudit;
        }
        #endregion
    }
}
