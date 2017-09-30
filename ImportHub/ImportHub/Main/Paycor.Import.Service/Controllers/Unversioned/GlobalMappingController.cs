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

namespace Paycor.Import.Service.Controllers.Unversioned
{
    [RoutePrefix("importhub")]
    public class GlobalMappingController : BaseSavedMappingController<GlobalMapping>
    {
        public GlobalMappingController(
            ILog log, 
            IDocumentDbRepository<GlobalMapping> repository, 
            ITableStorageProvider tableStorageProvider, 
            IValidator<GlobalMapping> validator) : 
            base(log, repository, tableStorageProvider, validator)
        {
        }

        private class UserMapPrivileges
        {
            public bool HasGlobalMappingPrivilege { get; set; }
            public bool HasClientMappingPrivilege { get; set; }
        }

        [SwaggerResponse(HttpStatusCode.OK, type:(typeof(string)))]
        [HttpGet]
        [Route("mappings/usersaveaccess")]
        public IHttpActionResult GetUserAccess()
        {
            return Ok(new UserMapPrivileges
            {
                HasClientMappingPrivilege = HasClientMappingPrivilege(),
                HasGlobalMappingPrivilege = HasGlobalMappingPrivilege()
            });
        }

        /// <summary>
        /// Returns the list of global mappings.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(IEnumerable<GlobalMapping>))]
        [HttpGet]
        [Route("globalmappings")]
        public override IHttpActionResult Get() => base.Get();

        /// <summary>
        /// Returns the specified global mapping
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(GlobalMapping))]
        [HttpGet]
        [Route("globalmappings/{id}")]
        public override IHttpActionResult Get(string id) => base.Get(id);

        /// <summary>
        /// Saves the specified global mapping.
        /// </summary>
        /// <param name="mapping">Takes GlobalMapping as input.</param>
        /// <returns>Status code with message.</returns>
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("globalmappings")]
        public override async Task<IHttpActionResult> Post(GlobalMapping mapping)
        {
            if (!HasGlobalMappingPrivilege())
            {
                return UnauthorizedAccessResponse();
            }

            // Ensure map name is unique across all global saved maps.
            var items = Repository.GetItems(x => x.MappingName.ToLower() == mapping.MappingName.ToLower());
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
        /// Updates the specified global mapping.
        /// </summary>
        /// <param name="id">The id of the mapping to update.</param>
        /// <param name="mapping">Takes mapping as input.</param>
        /// <returns>status code with message.</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("globalmappings/{id}")]
        public override IHttpActionResult Put(string id, GlobalMapping mapping)
        {
            if (!HasGlobalMappingPrivilege())
            {
                return UnauthorizedAccessResponse();
            }
            return base.Put(id, mapping);
        }

        /// <summary>
        /// Deletes the specified global mapping.
        /// </summary>
        /// <param name="id">the id of the mapping to delete</param>
        /// <returns><see cref="IHttpActionResult"/>The result of the operation</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Gone, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, type: typeof(ErrorResponse))]
        [Route("globalmappings/{id}")]
        [HttpDelete]
        public override IHttpActionResult Delete(string id)
        {
            if (!HasGlobalMappingPrivilege())
            {
                return UnauthorizedAccessResponse();
            }
            return base.Delete(id);
        }

        //will be deleted once deployed to QSB.
        //[HttpDelete]
        //[SwaggerResponse(HttpStatusCode.OK)]
        //[Route("globalmappings/deleteall")]
        //public override IHttpActionResult DeleteAll()
        //{
        //    if (!HasGlobalMappingPrivilege())
        //    {
        //        return UnauthorizedAccessResponse();
        //    }
        //    return base.DeleteAll();
        //}
    }
}
