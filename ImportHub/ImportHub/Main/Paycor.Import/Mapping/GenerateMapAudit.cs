using System;

namespace Paycor.Import.Mapping
{
    public class GenerateMapAudit
    {
        public ApiMappingAudit CreateMapAudit(DeleteMapKeys deleteMapKeys, string action, string id, string userName)
        {
            var dt = DateTime.Now;
            var dateTime =
                $"{dt:d-M-yyyy HH:mm:ss}";
            var mapAudit = new ApiMappingAudit(deleteMapKeys.MappingName.ToLower(),
                dateTime)
            {
                Action = action,
                MappingId = id,
                Reason = string.Empty,
                UserName = userName,
                MappingName = deleteMapKeys.MappingName,
                CurrentDateTime = dateTime
            };

            return mapAudit;
        }

        public ApiMappingAudit CreateMapAudit(ConvertMapKeys convertMapKeys, string action, string id, string userName)
        {
            var dt = DateTime.Now;
            var dateTime =
                $"{dt:d-M-yyyy HH:mm:ss}";
            var mapName =
                $"{convertMapKeys.CurrentMappingName.ToLower()}:{convertMapKeys.ConvertToMappingName.ToLower()}";
            var mapAudit = new ApiMappingAudit(mapName,
                dateTime)
            {
                Action = action,
                MappingId = id,
                Reason = string.Empty,
                UserName = userName,
                MappingName = mapName,
                CurrentDateTime = dateTime
            };

            return mapAudit;
        }
    }
}
