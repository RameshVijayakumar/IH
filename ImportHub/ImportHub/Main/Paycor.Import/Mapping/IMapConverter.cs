using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paycor.Import.Mapping
{
    public interface IMapConverter
    {
        Task<Dictionary<string, string>> ConvertMaps(ConvertMapInfo convertMapInfo);
    }
}