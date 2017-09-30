using System;
using System.Collections.Generic;

namespace Paycor.Import.MapFileImport
{
    public class BatchImportResponse
    {
        public Guid CorrelationId { get; set; }
       
        public IEnumerable<ErrorResponseDataBatch> ErrorResponseDataBatch;
    }

    public class ErrorResponseDataBatch
    {
        public int BatchLine { get; set; }
        public string Status { get; set; }
        public ErrorResponseBatchRowEntry ErrorResponseBatchRowEntry { get; set; }
    }

    public class ErrorResponseBatchRowEntry
    {
        public string Title { get; set; }

        public string Detail { get; set; }

        public Dictionary<string, string> Source { get; set; }
    }
}
