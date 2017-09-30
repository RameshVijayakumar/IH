using System;

namespace Paycor.Import.Employee
{
    public class ImportAnalysisRow
    {
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }

        public TimeSpan? ElapsedTime { get; set; }

        public TimeSpan? AverageTimeProcessPerRow { get; set; }

        public int TotalRecords { get; set; }

        public int SuccessRecords { get; set; }

        public int ErrorRecords { get; set; }

        public int CancelledRecords { get; set; }

        public string OriginatingUser { get; set; }

        public string ImportType { get; set; }   

        public string ClientId { get; set; }

        public string UserName { get; set; }
    }
}
