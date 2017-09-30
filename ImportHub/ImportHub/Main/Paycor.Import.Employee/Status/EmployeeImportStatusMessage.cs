using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Paycor.Import.Status;

namespace Paycor.Import.Employee.Status
{
    [ExcludeFromCodeCoverage]
    public class EmployeeImportStatusMessage : ImportStatusMessage
    {
        public EmployeeImportStatusEnum Status;
        public IList<EmployeeImportStatusDetail> StatusDetails;
        public EmployeeImportStatusMessage()
        {
            StatusDetails = new List<EmployeeImportStatusDetail>();
        }
        public string SummaryErrorMessage { get; set; }
    }

    public class EmployeeImportStatusDetail
    {
        public string EmployeeNumber;
        public string EmployeeName;
        public string Issue;
        public bool? RecordUploaded;
        public string IssueType;
    }
}