using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.Registration;
using Paycor.Import.Web;
using Swashbuckle.Swagger.Annotations;

namespace Paycor.Import.Service.Controllers.Unversioned
{
    [RoutePrefix("importhub")]
    public class MappingCertificationController : ApiController
    {
        #region Fields

        private readonly ILog _log;
        private readonly IMappingCertification _mappingCertification;

        #endregion

        public MappingCertificationController(ILog log, IMappingCertification mappingCertification)
        {
            Ensure.ThatArgumentIsNotNull(mappingCertification, nameof(mappingCertification));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
            _mappingCertification = mappingCertification;

        }

        /// <summary>
        /// Used to certify the mapping. 
        /// </summary>
        /// <param name="docUrl">Swagger DocUrl</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(bool))]
        [SwaggerResponse(HttpStatusCode.NotFound, type: typeof(ErrorResponse))]
        [Route("mappingcertification/docUrl")]
        public IHttpActionResult Get(string docUrl)
        {
            try
            {
                if (docUrl == null)
                    return Ok(HttpStatusCode.NotFound);
                string swaggerText;
                var client = new WebApiClient();
                try
                {
                   swaggerText = client.Get(docUrl);
                }
                catch (Exception ex)
                {
                    var response = new Dictionary<string, string> { ["Error"] = ex.Message };
                    return this.HtmlResponse(HttpStatusCode.BadRequest, "Not Found", "DocUrl not found.", response);
                }
                

                if (swaggerText == null)
                    return Ok(HttpStatusCode.NotFound);

                _log.Info($"Certification of {docUrl} started.");
                var isCertified = _mappingCertification.IsAllMappingCertified(swaggerText);
                if (!isCertified)
                {
                    _log.Error($"#RegistrationFailed Mappings of {docUrl} failed the certification and cannot be registered with ImportHub");
                    
                }
                _log.Info($"{docUrl} passed the certification.");
                return Ok(isCertified);
            }
            catch (Exception exception)
            {
                var response = new Dictionary<string, string> {["Error"] = exception.Message};
                _log.Error("Unable to get registered mappings.", exception);
                return this.HtmlResponse(HttpStatusCode.InternalServerError, "An error occurred",
                    "Unable to get registered mappings", response);
            }
        }
    }
}
