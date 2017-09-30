using System.Collections.Generic;

namespace Paycor.Import.FailedRecordFormatter
{
    public class FailedRecord
    {
        public IDictionary<string, string> Record { get; set; }
        public IDictionary<string, string> Errors { get; set; }
        public IDictionary<string, string> CustomData { get; set; }
    }
}
