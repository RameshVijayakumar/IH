using Paycor.Import.Extensions;
using Paycor.Import.Status;

namespace Paycor.Import.Http
{
    public interface IHttpStatusHandler
    {
        void SaveStatusFromApi(MappedFileImportStatusMessage statusMessage, MappedFileImportStatusLogger statusLogger, int rowNumber, HttpExporterResult result, ErrorResponse errorResponse = null);
        void AddErrorStatusDetails(MappedFileImportStatusMessage statusMessage, ErrorResponse errorResponse, int rowNumber, HttpExporterResult result);
        ErrorResponse GetErrorResponse(HttpExporterResult result, MappedFileImportStatusMessage statusMessage);
    }
}