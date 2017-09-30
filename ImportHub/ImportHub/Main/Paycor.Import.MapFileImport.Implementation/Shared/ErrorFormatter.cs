using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation.Shared
{
    public class ErrorFormatter : IErrorFormatter
    {
        const int MaximumLength = 120;
        public string FormatError(ErrorResponse errorResponse, HttpExporterResult result)
        {           
            return
                errorResponse.Detail?.CheckGreaterThanLength(MaximumLength) ??
                errorResponse.Title?.CheckGreaterThanLength(MaximumLength) ??
                result.Response?.ReasonPhrase?.CheckGreaterThanLength(MaximumLength) ??
                result.Response?.StatusCode.ToString();
        }
    }
}
