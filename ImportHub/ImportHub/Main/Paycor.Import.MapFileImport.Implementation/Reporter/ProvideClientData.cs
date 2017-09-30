using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;

namespace Paycor.Import.MapFileImport.Implementation.Reporter
{
    public class ProvideClientData : IProvideClientData<MapFileImportResponse>
    {
        public IEnumerable<string> GetAllClientIds(MapFileImportResponse response)
        {
            var chunkDataResponse = response as ChunkDataResponse;
            var chunkMultiDataResponse = response as ChunkMultiDataResponse;

            if (null != chunkDataResponse)
            {
                return GetClientIdsFromChunk(chunkDataResponse);
            }

            if (null != chunkMultiDataResponse)
            {
                return GetClientIdsFromMultiChunk(chunkMultiDataResponse);
            }

            var chunkbuildDataResponse = response as BuildDataSourceResponse;

            return null != chunkbuildDataResponse ? GetClientIdsFromChunk(chunkbuildDataResponse) : new List<string>();
        }

        private static IEnumerable<string> GetClientIdsFromChunk(BuildDataSourceResponse chunkDataResponse)
        {
            var clientIds = new List<string>();
            if (chunkDataResponse.DataSource == null) return clientIds;

            foreach (var chunk in chunkDataResponse.DataSource.Records)
            {
                var clientId = chunk.GetClientId();

                if (!string.IsNullOrWhiteSpace(clientId) && !clientIds.Contains(clientId))
                    clientIds.Add(clientId);
            }

            return clientIds;
        }

        private static IEnumerable<string> GetClientIdsFromChunk(ChunkDataResponse chunkDataResponse)
        {
            var clientIds = new List<string>();
            if (chunkDataResponse.Chunks == null) return clientIds;
            foreach (var chunk in chunkDataResponse.Chunks)
            {
                var records = chunk.ToList();
                clientIds.AddRange(records.Select(t => t.GetClientId())
                    .Where(t => !string.IsNullOrWhiteSpace(t)));
            }

            return clientIds;
        }

        private static IEnumerable<string> GetClientIdsFromMultiChunk(ChunkMultiDataResponse chunkMultiDataResponse)
        {
            var clientIds = new List<string>();
            if (chunkMultiDataResponse.Chunks == null) return clientIds;
            foreach (var chunk in chunkMultiDataResponse.Chunks)
            {
                var records = chunk.Value.ToList();
                clientIds.AddRange(records.Select(t => t.GetClientId())
                    .Where(t => !string.IsNullOrWhiteSpace(t)));
            }

            return clientIds;
        }
    }
}
