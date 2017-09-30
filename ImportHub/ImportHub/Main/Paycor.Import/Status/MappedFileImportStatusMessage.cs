using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.Status
{
    public class MappedFileImportStatusMessage : ImportStatusMessage
    {
        public MappedFileImportStatusEnum Status { get; set; }

        public string IssueId { get; set; }

        public string IssueStatus { get; set; }

        public string IssueTitle { get; set; }

        public string IssueDetail { get; set; }

        public string ApiLink { get; set; }

        public IEnumerable<ApiMapping> ApiMappings { get; set; }

        public IList<MappedFileImportErrorSourceDetail> StatusDetails { get; set; }

        public MappedFileImportStatusMessage()
        {
            StatusDetails = new List<MappedFileImportErrorSourceDetail>();
        }
    }

    public class MappedFileImportErrorSourceDetail
    {
        public int RowNumber { get; set; }

        public string ColumnName { get; set; }

        public string Issue { get; set; }

        public string ImportType { get; set; }
    }
}