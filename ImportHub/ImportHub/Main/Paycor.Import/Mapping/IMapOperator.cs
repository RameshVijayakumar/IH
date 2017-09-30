using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paycor.Import.Mapping
{
    public interface IMapOperator
    {
        MapType MapType { get; }
        Task DeleteMapAsync(DeleteMapKeys deleteMapKeys, Dictionary<string, string> errorsList);
        Task SaveMapAsync(string map);
        Task DeleteMapAsync(string id);

        Task ConvertMapAsync(ConvertMapKeys convertMapKeys,string map, Dictionary<string, string> errorsList);
    }
}