using Paycor.Import.Extensions;
using Paycor.Import.Http;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IErrorFormatter
    {
        string FormatError(ErrorResponse errorResponse, HttpExporterResult result);
    }
}
