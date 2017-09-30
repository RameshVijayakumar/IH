using Microsoft.WindowsAzure.Storage.Table;

namespace Paycor.Import.Mapping
{
    public class ApiMappingAudit : TableEntity
    {
        public string MappingName { get; set; }

        public string UserName { get; set; }

        public string MappingId { get; set; }

        public string CurrentDateTime { get; set; }
        public string Action { get; set; }

        public string Reason { get; set; }

        public ApiMappingAudit(string mapName, string dateTime)
        {
            PartitionKey = mapName;
            RowKey = dateTime;
        }

        public ApiMappingAudit() {}
    }
}
