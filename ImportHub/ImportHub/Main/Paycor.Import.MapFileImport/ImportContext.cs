using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport
{
    [ExcludeFromCodeCoverage]
    public class ImportContext
    {

        public string MasterSessionId { get; set; }

        public string TransactionId { get; set; }

        public string Container { get; set; }

        public string FileName { get; set; }

        public string UploadedFileName { get; set; }

        public int ChunkSize { get; set; }

        public int ChunkNumber { get; set; }

        public int XlsxWorkSheetNumber { get; set; }

        public List<string> ColumnHeaders { get; set; }

        public bool HasHeader { get; set; }

        public Dictionary<HtmlVerb, string> Endpoints { get; set; }

        public bool CallApiInBatch { get; set; }

        public bool IsMultiSheetImport { get; set; }

        public IEnumerable<ApiMapping> ApiMapping { get; set; }

        public string PaycorUserKey { get; set; }

        public IDictionary<string, string> ImportHeaderInfo { get; set; }

        public int TotalTabs { get; set; }

        public int? TotalRecordCount { get; set; }

        public bool DelayProcess { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
