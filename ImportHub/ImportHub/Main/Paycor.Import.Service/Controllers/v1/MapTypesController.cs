
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.FileType;
using Paycor.Import.ImportHistory;
using Paycor.Import.Mapping;
using Paycor.Import.Service.FileType;
using Paycor.Import.Service.Shared;
using Paycor.Security.Principal;
using Swashbuckle.Swagger.Annotations;

namespace Paycor.Import.Service.Controllers.v1
{
    [Authorize]
    [RoutePrefix("importhub/v1")]
    public class MapTypesController : ApiController
    {
        private readonly ILog _log;
        private readonly IFileTypeHandlerFactory _factory;

        public MapTypesController(ILog log, IFieldMapper fieldMapper, IImportHistoryService importHistoryService)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(fieldMapper, nameof(fieldMapper));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));

            _log = log;
            _factory = new ImportHubFileTypeHandlerFactory(log, importHistoryService, fieldMapper);
        }

        [HttpPost]
        [Route("maptypes")]
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(IEnumerable<GeneratedMapping>))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, type: typeof(ErrorResponse))]
        public IHttpActionResult Post(FileTypeFilteredPayload filteredPayload, FileTypeSortOrder
                sortOrder = FileTypeSortOrder.Alpha,
            AlgorithmType algorithmType = AlgorithmType.Legacy)
        {
            Ensure.ThatArgumentIsNotNull(filteredPayload, nameof(filteredPayload));
            var principal = HttpContext.Current.User as PaycorUserPrincipal;
            try
            {

                var rows = filteredPayload.Payload.Where(row => !string.IsNullOrWhiteSpace(row)).ToList();
                var header = rows.First();
                rows.RemoveAt(0);

                var fileType = FileTypeRecognizer.DetermineFormat(header);
                if (fileType != FileTypeEnum.MappedFileImport)
                {
                    var validationResponse = new Dictionary<string, string>
                    {
                        ["Not Supported"] =
                        $"Operation not supported on non mapped file types. File type detected as {fileType}."
                    };
                    return this.ValidationResponse(validationResponse);
                }
                var handler = _factory.GetFileTypeHandler(fileType);
                var info =
                    (MappedFileTypeInfo)
                    handler.GetTypeInfo(header, principal, rows, filteredPayload.ObjectType, sortOrder, algorithmType);
                var mapping = info.Mappings;
                return Ok(mapping);
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(MapTypesController)}:{nameof(Post)} action", ex);
                return this.ExceptionResponse(ex);
            }
        }
    }
}
