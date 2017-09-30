using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.Extensions
{
    public static class ClientMappingExtensions
    {
        public static void UpdateGeneratedMappingName(this IEnumerable<ClientMapping> mappings)
        {
            if (mappings == null) return;
            foreach (var clientMapping in mappings)
            {
                if (clientMapping != null)
                {
                    clientMapping.GeneratedMappingName =
                        clientMapping.MappingName?.GetClientGeneratedMappingName(clientMapping?.ClientId);
                }
            }
        }
    }
}
