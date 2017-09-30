using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.Http;

namespace Paycor.Import.MapFileImport
{
    public enum Status
    {
        Success,
        Failure,
        Cancel 
    }

    [ExcludeFromCodeCoverage]
    public class MapFileImportResponse
    {
        public Status Status { get; set; }

        public Exception Error { get; set; }

        public IEnumerable<ErrorResultData> ErrorResultDataItems { get; set; }

        public int TotalRecordsCount { get; set; }

        public int TotalChunks { get; set; }

        public int PayloadCount { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ErrorResultData
    {
        public ErrorResponse ErrorResponse { get; set; }

        public int RowNumber { get; set; }

        public FailedRecord FailedRecord { get; set; }

        public HttpExporterResult HttpExporterResult { get; set; }

        public string ImportType { get; set; }
    }   
}