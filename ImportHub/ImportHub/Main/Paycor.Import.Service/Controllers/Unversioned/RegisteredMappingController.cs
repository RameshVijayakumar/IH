
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using log4net;
using Microsoft.ServiceBus.Messaging;
using Paycor.Import.Attributes;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Registration.Client;
using Paycor.Security.Principal;
using Swashbuckle.Swagger.Annotations;
using ErrorResponse = Paycor.Import.Extensions.ErrorResponse;
// ReSharper disable All

namespace Paycor.Import.Service.Controllers.Unversioned
{
    [Authorize]
    [RoutePrefix("importhub")]
    public class RegisteredMappingController : ApiController
    {
        #region Fields

        private readonly ILog _log;
        private readonly PaycorUserPrincipal _principal;
        private readonly IDocumentDbRepository<GeneratedMapping> _apiMappingRepository;

        #endregion

        public RegisteredMappingController(ILog log, 
            IDocumentDbRepository<GeneratedMapping> apiMappingRepository)
        {
            Ensure.ThatArgumentIsNotNull(apiMappingRepository, nameof(apiMappingRepository));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
            _principal = HttpContext.Current.User as PaycorUserPrincipal;
            _apiMappingRepository = apiMappingRepository;
        }

        #region RegisteredMaps Endpoints
        /// <summary>
        /// Gets all mappings that have been registered (a.k.a. "generated") with ImportHub.
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(IEnumerable<GeneratedMapping>))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [RolesAuthorize(ImportConstants.PaycorRole)]
        [Route("registeredmaps")]
        public IHttpActionResult Get()
        {
            var docUrl = GetDocUrlFromQueryString();
            try
            {
                var mapQuery = _apiMappingRepository.GetQueryableItems();
                if (docUrl != null)
                {
                    mapQuery = mapQuery.Where(t => t.DocUrl.ToLower() == docUrl.ToLower());
                }

                var apiMappings = mapQuery.AsEnumerable();
                if (apiMappings.Any())
                {
                    apiMappings.UpdateGeneratedMappingName();
                    return Ok(apiMappings);
                }

                return this.HtmlResponse(HttpStatusCode.NotFound, "Not found", "No registered mappings exist.", null);
            }
            catch (Exception exception)
            {
                _log.Error("Unable to get registered mappings.", exception);
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Sends a request to re-register maps with ImportHub with the provided docUrl sent
        /// as a querystring parameter.
        /// </summary>
        /// <returns></returns>
        [RolesAuthorize(ImportConstants.PaycorRole)]
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(IEnumerable<GeneratedMapping>))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [Route("registeredmaps")]
        [RolesAuthorize(ImportConstants.PaycorRole)]
        public IHttpActionResult Post()
        {
            var docUrl = GetDocUrlFromQueryString();

            try
            {
                var connectionString =
                    ConfigurationManager.AppSettings[RegistrationServiceTopicInfo.ServiceBusConnectionKey];
                var client = TopicClient.CreateFromConnectionString(connectionString, RegistrationServiceTopicInfo.TopicName);

                if (docUrl != null)
                {
                    client.Send(new BrokeredMessage(docUrl));
                    _log.Info($"{_principal.UserKey} has reset mappings for {docUrl}.");
                    return Ok();
                }
                return this.HtmlResponse(HttpStatusCode.BadRequest, "No Url Specified",
                    "In order to use this method, the DocUrl querystring parameter must be specified.", null);
            }
            catch (Exception exception)
            {
                _log.Error($"Unable to reset registered mappings for {docUrl}", exception);
                return InternalServerError(exception);
            }
        }

        [HttpGet]
        [Route("lookups/docurls")]
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(Dictionary<string, string>))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [RolesAuthorize(ImportConstants.PaycorRole)]
        public IHttpActionResult GetMapsWithLookup(string commaSeparatedLookupParams)
        {
            if(string.IsNullOrEmpty(commaSeparatedLookupParams))
            {
                return this.HtmlResponse(HttpStatusCode.BadRequest, "Cannot be null",
                    $"{commaSeparatedLookupParams} cannot be null", null);
            }
            var result = new Dictionary<string, string>();
            var listOfLookupParams = commaSeparatedLookupParams.Split(',').ToList();
            var regMaps = _apiMappingRepository.GetItems();
            foreach(var map in regMaps)
            {
                var lookupParams = map.Mapping.GetAllLookupSourceFields().Select(t=>t.RemoveBraces());
                if(listOfLookupParams.ExistIn(lookupParams))
                {
                    result[map.MappingName] = map.DocUrl;              
                }
            }
            if(!result.Any())
                return this.HtmlResponse(HttpStatusCode.NotFound, "No mapping found",
                    $"No maps found for the given lookup parameters {commaSeparatedLookupParams}", null);

            return Ok(result);

        }
        #endregion

        #region Private methods
        private string GetDocUrlFromQueryString()
        {
            var querystrings = Request.GetQueryNameValuePairs().ToDictionary(
                x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            string docUrl;
            querystrings.TryGetValue("DocUrl", out docUrl);
            return docUrl;
        }
        #endregion
    }
}
