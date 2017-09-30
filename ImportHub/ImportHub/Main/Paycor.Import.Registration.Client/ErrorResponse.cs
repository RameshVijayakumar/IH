using System;
using System.Collections.Generic;

namespace Paycor.Import.Registration.Client
{
    public class ErrorResponse
    {
        public Guid CorrelationId { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public IDictionary<string, string> Source { get; set; }
    }
}