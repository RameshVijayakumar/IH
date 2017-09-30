using System;
using System.Collections.Generic;
using System.Linq;
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

// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.Service.Controllers.Unversioned
{
    [Authorize]
    [RoutePrefix("importhub")]
    public class FileTypesController : ApiController
    {
        private readonly ILog _log;
        private readonly IFileTypeHandlerFactory _factory;

        public FileTypesController(ILog log, IFieldMapper fieldMapper, IImportHistoryService importHistoryService)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(fieldMapper, nameof(fieldMapper));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));

            _log = log;
            _factory = new ImportHubFileTypeHandlerFactory(log, importHistoryService, fieldMapper);
        }

        [HttpPost]
        [Route("filetypes")]
        public IHttpActionResult Post(
            IEnumerable<IEnumerable<string>> payload,
            FileTypeSortOrder sortOrder = FileTypeSortOrder.Percentage)
        {

            var principal = HttpContext.Current.User as PaycorUserPrincipal;
            try
            {
                var position = FileTypeRecognizer.IndexOfValidData(payload);
                if (position == -1)
                {
                    throw new ImportException(ExceptionMessages.MissingData);
                }

                var header = payload.ElementAt(position).FirstOrDefault();
                var fileType = FileTypeRecognizer.DetermineFormat(header);
                var handler = _factory.GetFileTypeHandler(fileType);

                var sampleData = payload.Select(load => load.FirstOrDefault()).ToList();
                return
                    Ok(handler.GetTypeInfo(header, principal, sampleData, algorithmType: AlgorithmType.FileAndApiBased, sortOrder: sortOrder));
            }
            catch (Exception ex)
            {
                _log.Error($"An Error Occurred in {nameof(FileTypesController)}:{nameof(Post)} action", ex);
                return this.ExceptionResponse(ex);
            }
        }
    }
}