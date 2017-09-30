using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Validator;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Paycor.Security.Principal;

namespace Paycor.Import.Service.Controllers.Unversioned
{
    [RoutePrefix("importhub")]
    public class UserMappingController : BaseSavedMappingController<UserMapping>
    {

        public UserMappingController(
            ILog log,
            IDocumentDbRepository<UserMapping> repository,
            ITableStorageProvider tableStorageProvider, IValidator<UserMapping> validator) :
                base(log, repository, tableStorageProvider, validator)
        {
        }

        /// <summary>
        /// Returns the list of user mappings for the current user.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(IEnumerable<UserMapping>))]
        [HttpGet]
        [Route("usermappings")]
        public IHttpActionResult Get(bool allMaps = false)
        {

            var user = Principal?.UserKey.ToString().ToLower();
            if (string.IsNullOrEmpty(user))
            {
                var noUser = new Dictionary<string, string>
                {
                    ["user"] = "unspecified or unavailable. must be authenticated."
                };
                return this.ValidationResponse(noUser);
            }
            var items = allMaps ? Repository.GetItems() : Repository.GetItems(x => user == x.User.ToLower());
            return Ok(items);
        }

        /// <summary>
        /// Returns the specified user mapping.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(UserMapping))]
        [HttpGet]
        [Route("usermappings/{id}")]
        public override IHttpActionResult Get(string id)
        {
            var user = Principal?.UserKey.ToString();
            id = id.ToLower();
            if (string.IsNullOrEmpty(user))
            {
                var noUser = new Dictionary<string, string>
                {
                    ["user"] = "unspecified or unavailable. must be authenticated."
                };
                return this.ValidationResponse(noUser);
            }
            var item = Repository.GetItem(x => id == x.Id.ToLower());
            if (item == null)
            {
                var srcNotFound = new Dictionary<string, string>
                {
                    ["Id"] = "item not found"
                };
                return this.HtmlResponse(HttpStatusCode.NotFound, "Not Found", "The requested item was not found.",
                    srcNotFound);
            }
            if (!user.Equals(item.User, StringComparison.OrdinalIgnoreCase))
            {
                return UnauthorizedAccessResponse();
            }
            return Ok(item);
        }

        /// <summary>
        /// Saves the specified user mapping.
        /// </summary>
        /// <param name="mapping">Takes UserMapping as input.</param>
        /// <returns>Status code with message.</returns>
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("usermappings")]
        public override async Task<IHttpActionResult> Post(UserMapping mapping)
        {
            mapping.User = Principal?.UserKey.ToString();
            if (string.IsNullOrEmpty(mapping.User))
            {
                var userNull = new Dictionary<string, string> {["User"] = "cannot be null or empty"};
                return this.ValidationResponse(userNull);
            }

            // Ensure map name is unique for the current user.
            var items = Repository.GetItems(x => x.User == mapping.User &&
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
        /// Clones the specified user mapping.
        /// </summary>
        /// <param name="mapping">Takes CloneUserMapping as input.</param>
        /// <returns>Status code with message.</returns>
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("cloneuserMapping")]
        [HttpPost]
        public async Task<IHttpActionResult> Clone(CloneUserMapping mapping)
        {
            if (!HasMapManagementPrivilege())
            {
                return UnauthorizedAccessResponse();
            }

            var userMapping = Repository.GetItem(x => x.User == mapping.User &&
                                                 x.MappingName.ToLower() == mapping.MappingName.ToLower());
            if (userMapping== null)
            {
                var mapNameUnique = new Dictionary<string, string>
                {
                    [nameof(mapping.MappingName)] = $"'{mapping.MappingName}' is not available to be cloned."
                };
                return this.ValidationResponse(mapNameUnique);
            }

            var clonedUserMapping = Repository.GetItem(x => x.User == mapping.CloneToUser &&
                                                        x.MappingName.ToLower() == mapping.MappingName.ToLower());
            if (clonedUserMapping != null)
            {
                var mapNameUnique = new Dictionary<string, string>
                {
                    [nameof(mapping.MappingName)] = $"'{mapping.MappingName} for user:{mapping.CloneToUser}' is already in use. Please select another."
                };
                return this.ValidationResponse(mapNameUnique);
            }

            userMapping.User = mapping.CloneToUser;
            return await base.Post(userMapping);
        }


        /// <summary>
        /// Updates the specified user mapping.
        /// </summary>
        /// <param name="id">The id of the mapping to update.</param>
        /// <param name="mapping">Takes mapping as input.</param>
        /// <returns>status code with message.</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("usermappings/{id}")]
        public override IHttpActionResult Put(string id, UserMapping mapping)
        {
            var user = Principal?.UserKey.ToString();

            if (string.IsNullOrEmpty(mapping.User))
            {
                var notAUserMap = new Dictionary<string, string> {["Cannot Modify"] = ":You are not authorized to modify or delete this mapping."};
                return this.HtmlResponse(HttpStatusCode.Forbidden, "User not present", "Map is not a user map", notAUserMap);
            }
            if (!user.Equals(mapping.User, StringComparison.OrdinalIgnoreCase))
            {
                return UnauthorizedAccessResponse();
            }
            return base.Put(id, mapping);
        }

        /// <summary>
        /// Deletes the specified user mapping.
        /// </summary>
        /// <param name="id">the id of the mapping to delete</param>
        /// <returns><see cref="IHttpActionResult"/>The result of the operation</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Gone, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("usermappings/{id}")]
        public override IHttpActionResult Delete(string id)
        {
            var user = Principal?.UserKey.ToString();
            var map = Repository.GetItem(x => x.Id == id);
            if (map == null)
            {
                var idNotFound = new Dictionary<string, string> {["Id"] = "Unable to delete - not found"};
                return this.ValidationResponse(idNotFound);
            }
            if (!user.Equals(map.User, StringComparison.OrdinalIgnoreCase))
            {
                var notCurrentUser = new Dictionary<string, string>
                {
                    ["User"] = "The specified map does not belong to the current user."
                };
                return this.ValidationResponse(notCurrentUser);
            }
            return base.Delete(id);
        }

        /// <summary>
        /// Gets all the maps for the specified user.
        /// </summary>
        /// <param name="userkey">the userkey of the user</param>
        /// <returns><see cref="IHttpActionResult"/>The result of the operation</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Gone, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [Route("usermappings/user/{userkey}")]
        public IHttpActionResult GetUserMaps(string userkey)
        {
            if (string.IsNullOrEmpty(userkey))
            {
                var noUser = new Dictionary<string, string>
                {
                    ["user"] = "Please provide a user key."
                };
                return this.ValidationResponse(noUser);
            }
            var notFound = new Dictionary<string, string>
            {
                ["Not Found"] = "No maps found for this user."
            };
            var userMaps = Repository.GetItems(x => userkey == x.User.ToLower());
            return !userMaps.Any() ? this.ValidationResponse(notFound) : Ok(userMaps);
        }


        //will be deleted once deployed to QSB.
        //[HttpDelete]
        //[SwaggerResponse(HttpStatusCode.OK)]
        //[Route("usermappings/deleteall")]
        //public override IHttpActionResult DeleteAll()
        // {
        //     return base.DeleteAll();
        // }

    }
}
