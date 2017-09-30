using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Validator;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
// ReSharper disable All

namespace Paycor.Import.Service.Controllers.Unversioned
{
    [RoutePrefix("importhub")]
    public class ClientMappingController : BaseSavedMappingController<ClientMapping>
    {
        public ClientMappingController(
            ILog log,
            IDocumentDbRepository<ClientMapping> repository,
            ITableStorageProvider tableStorageProvider, IValidator<ClientMapping> validator) :
            base(log, repository, tableStorageProvider, validator)
        { }

        /// <summary>
        /// Returns the list of client mappings.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(IEnumerable<ClientMapping>))]
        [HttpGet]
        [Route("clientmappings")]
        public IHttpActionResult Get(bool allMaps = false)
        {
            var user = Principal;
            var rls = user.GetRowLevelSecurity(PrivilegeConstants.ClientMapPrivilegeId);

            if (!allMaps && rls.DoesNotHaveAccessToAnyClient())
                return UnauthorizedAccessResponse();

            var maps = Repository.GetItems();

            if (allMaps)
            {
                maps.UpdateGeneratedMappingName();
                return Ok(maps);
            }

            if (rls.DoesHaveAccessToAllClients())
            {
                maps.UpdateGeneratedMappingName();
                return Ok(maps);
            }

            var clients = rls.GetListofPrivilegeClients();
            maps = maps.Where(t => clients.Contains(t.ClientId));
            maps.UpdateGeneratedMappingName();

            return Ok(maps);
        }

        /// <summary>
        /// Returns the specified client mapping.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(IEnumerable<ClientMapping>))]
        [HttpGet]
        [Route("clientmappings/{id}")]
        public override IHttpActionResult Get(string id)
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
            int clientId;
            if (!int.TryParse(mapping.ClientId, out clientId))
            {
                var noClientId = new Dictionary<string, string>
                {
                    ["clientId"] = "is required and must be an integer value"
                };
                return this.ValidationResponse(noClientId);
            }
            if (!HasClientMappingPrivilege(clientId))
            {
                return UnauthorizedAccessResponse();
            }

            mapping.GeneratedMappingName = mapping.MappingName.GetClientGeneratedMappingName(id);
            
            return Ok(mapping);
        }

        /// <summary>
        /// Saves the specified client mapping.
        /// </summary>
        /// <param name="mapping">Takes UserMapping as input.</param>
        /// <returns>Status code with message.</returns>
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("clientmappings")]
        public override async Task<IHttpActionResult> Post(ClientMapping mapping)
        {
            int clientId = int.Parse(mapping.ClientId);

            if (!HasClientMappingPrivilege(clientId))
            {
                return UnauthorizedAccessResponse();
            }

            // Ensure map name is unique for the specified client.
            var items = Repository.GetItems(x => x.ClientId == mapping.ClientId &&
                x.MappingName.ToLower() == mapping.MappingName.ToLower());
            if (items.Any())
            {
                var mapNameUnique = new Dictionary<string, string>
                {
                    [nameof(mapping.MappingName)] = $"'{mapping.MappingName}' is already in use. Please select another."
                };
                return this.ValidationResponse(mapNameUnique);
            }
            return await base.Post(mapping);
        }


        /// <summary>
        /// Clones the specified client mapping.
        /// </summary>
        /// <param name="cloneClientMapping">Takes CloneClientMapping as input.</param>
        /// <returns>Status code with message.</returns>
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("cloneclientMapping")]
        [HttpPost]
        public async Task<IHttpActionResult> Post(CloneClientMapping cloneClientMapping)
        {
            if (!HasMapManagementPrivilege())
            {
                return UnauthorizedAccessResponse();
            }

            var clientMapping = Repository.GetItem(x => x.ClientId == cloneClientMapping.ClientId &&
                                                 x.MappingName.ToLower() == cloneClientMapping.MappingName.ToLower());
            if (clientMapping == null)
            {
                var mapNameUnique = new Dictionary<string, string>
                {
                    [nameof(cloneClientMapping.MappingName)] = $"'{cloneClientMapping.MappingName}' is not available to be cloned."
                };
                return this.ValidationResponse(mapNameUnique);
            }


            var clonedclientMapping = Repository.GetItem(x => x.ClientId == cloneClientMapping.CloneToClientId &&
                                                    x.MappingName.ToLower() == cloneClientMapping.MappingName.ToLower());
            if (clonedclientMapping != null)
            {
                var mapNameUnique = new Dictionary<string, string>
                {
                    [nameof(cloneClientMapping.MappingName)] = $"'{cloneClientMapping.MappingName} with clientId:{cloneClientMapping.CloneToClientId}' is already in use. Please select another."
                };
                return this.ValidationResponse(mapNameUnique);
            }
            clientMapping.ClientId = cloneClientMapping.CloneToClientId;
            return await base.Post(clientMapping);
        }

        /// <summary>
        /// Updates the specified client mapping.
        /// </summary>
        /// <param name="id">The id of the mapping to update.</param>
        /// <param name="mapping">Takes mapping as input.</param>
        /// <returns>status code with message.</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("clientmappings/{id}")]
        public override IHttpActionResult Put(string id, ClientMapping mapping)
        {
            int clientId;
            if (!int.TryParse(mapping.ClientId, out clientId))
            {
                var noClientId = new Dictionary<string, string>
                {
                    ["clientId"] = "is required and must be an integer value"
                };
                return this.ValidationResponse(noClientId);
            }
            if (!HasClientMappingPrivilege(clientId))
            {
                return UnauthorizedAccessResponse();
            }
            return base.Put(id, mapping);
        }
        /// <summary>
        /// Deletes the specified client mapping.
        /// </summary>
        /// <param name="id">the id of the mapping to delete</param>
        /// <returns><see cref="IHttpActionResult"/>The result of the operation</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("clientmappings/{id}")]
        [HttpDelete]
        public override IHttpActionResult Delete(string id)
        {
            var mapping = Repository.GetItem(map => map.Id == id);
            if (mapping == null)
            {
                var srcNotFound = new Dictionary<string, string>
                {
                    ["Id"] = "item not found"
                };
                return this.HtmlResponse(HttpStatusCode.NotFound, "Not Found", "The requested item was not found or already removed.", srcNotFound);
            }
            int clientId;
            if (!int.TryParse(mapping.ClientId, out clientId))
            {
                var noClientId = new Dictionary<string, string>
                {
                    ["clientId"] = "is required and must be an integer value"
                };
                return this.ValidationResponse(noClientId);
            }
            if (!HasClientMappingPrivilege(clientId))
            {
                return UnauthorizedAccessResponse();
            }
            return base.Delete(id);
        }


        /// <summary>
        /// Gets all the maps for the specified user.
        /// </summary>
        /// <param name="clientId">the userkey of the user</param>
        /// <returns><see cref="IHttpActionResult"/>The result of the operation</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Gone, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [Route("clientmappings/client/{clientId}")]
        public IHttpActionResult GetClientMaps(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                var noClientId = new Dictionary<string, string>
                {
                    ["ClientId"] = "Please provide a clientId."
                };
                return this.ValidationResponse(noClientId);
            }
            var notFound = new Dictionary<string, string>
            {
                ["Not Found"] = "No maps found for this clientId."
            };
            var clientMaps = Repository.GetItems(x=> x.ClientId == clientId);
            return !clientMaps.Any() ? this.ValidationResponse(notFound) : Ok(clientMaps);
        }

        //will be deleted once deployed to QSB.
        //[HttpDelete]
        //[SwaggerResponse(HttpStatusCode.OK)]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        //[Route("clientmappings/deleteall")]
        //public override IHttpActionResult DeleteAll()
        //{ 
        //    return base.DeleteAll();
        //}
    }
}
