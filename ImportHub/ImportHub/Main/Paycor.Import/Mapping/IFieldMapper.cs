using System.Collections.Generic;
using Paycor.Import.FileType;
using Paycor.Security.Principal;

namespace Paycor.Import.Mapping
{
    public interface IFieldMapper
    {
        ApiMapping FindMappingDefinition(IEnumerable<string> fields, PaycorUserPrincipal principal);

        IEnumerable<ApiMapping> GetMappingDefinitions(IEnumerable<string> fields,
            PaycorUserPrincipal principal,
            out IDictionary<string, int?> mapRankings,
            string objectType = null,
            AlgorithmType algorithmType = AlgorithmType.Legacy,
            FileTypeSortOrder sortOrder = FileTypeSortOrder.Alpha);

        IEnumerable<ApiMapping> GetAllApiMappings(PaycorUserPrincipal principal, string objectType = null);
    }
}
