using System;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.ImportHistory;
using Paycor.Import.Security;
using Paycor.Security.Principal;
using System.Net;
using System.Threading.Tasks;
using Paycor.Import.Employee.ImportHistory;
using Swashbuckle.Swagger.Annotations;
// ReSharper disable All

namespace Paycor.Import.Service.Controllers.v1
{
    /// <summary>
    /// A controller for the history of imported file.
    /// </summary>
    [Authorize]
    [RoutePrefix("importhub/v1")]
    public class HistoryController : ApiController
    {
        private readonly ILog _log;
        private readonly IImportHistoryService _importHistoryService;
        private SecurityValidator _securityValidator;
        private readonly PaycorUserPrincipal _principal;
        private const int HistoryMessagesDisplayCount = 25;
        private readonly ILegacyCleanUp _legacyCleanUp;

        private SecurityValidator SecurityValidator
        {
            get { return _securityValidator ?? (_securityValidator = new SecurityValidator(_log, _principal)); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="log">log for logging purpose.</param>
        /// <param name="importHistoryService">Keeps track of import history, gets import history by user and returns failed records based on each file imported.</param>
        /// <param name="legacyCleanUp"></param>
        public HistoryController(ILog log, IImportHistoryService importHistoryService, ILegacyCleanUp legacyCleanUp)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            Ensure.ThatArgumentIsNotNull(legacyCleanUp, nameof(legacyCleanUp));

            _log = log;
            _importHistoryService = importHistoryService;
            _principal = HttpContext.Current.User as PaycorUserPrincipal;
            _legacyCleanUp = legacyCleanUp;
        }

        /// <summary>
        /// Gets the history of imported files for the user.
        /// </summary>
        /// <returns>Returns the import history messages for the user.</returns>
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(IEnumerable<ImportHistoryMessage>))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [HttpGet]
        [Route("history")]
        public IHttpActionResult Get()
        {
            try
            {
                var user = SecurityValidator.GetUser();
                var historyMessages = _importHistoryService.GetImportHistoryByUser(user, HistoryMessagesDisplayCount);

                if (historyMessages != null)
                {
                    // TODO:- We could remove this at some point
                    _legacyCleanUp.UpdateStuckLegacyHistoryMessages(historyMessages);
                }

                return Ok(historyMessages);
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(HistoryController)}:{nameof(Get)} action", ex);
                return this.ExceptionResponse(ex);
            }
        }

        /// <summary>
        /// Deletes the history of imported files for the user.
        /// </summary>
        /// <returns><see cref="IHttpActionResult"/>The result of the operation</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [HttpDelete]
        [Route("history")]
        public async Task<IHttpActionResult> Delete()
        {
            try
            {
                var user = SecurityValidator.GetUser();
                await _importHistoryService.DeleteImportHistoryByUser(user);
                return Ok();
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(HistoryController)}:{nameof(Delete)} action", ex);
                return this.ExceptionResponse(ex);
            }
        }

        /// <summary>
        /// Deletes the history of imported files for the user.
        /// </summary>
        /// <returns><see cref="IHttpActionResult"/>The result of the operation</returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        [HttpDelete]
        [Route("history/{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            try
            {
                var user = SecurityValidator.GetUser();
                var isRemoved = await _importHistoryService.DeleteImportHistory(user, id);
                if (isRemoved) return Ok();
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(HistoryController)}:{nameof(Delete)} action", ex);
                return this.ExceptionResponse(ex);
            }
        }

    }
}
