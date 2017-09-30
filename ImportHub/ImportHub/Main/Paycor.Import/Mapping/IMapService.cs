using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paycor.Import.Mapping
{
    public interface IMapService
    {
        Task<Dictionary<string, string>> DeleteMapsAsync(DeleteMapInfo mapinfo);
        Task<Dictionary<string, string>> ConvertMapsAsync(ConvertMapInfo convertMapInfo);
    }
}