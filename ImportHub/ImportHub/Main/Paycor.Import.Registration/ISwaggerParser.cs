using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public interface ISwaggerParser
    {
        IEnumerable<GeneratedMapping> GetAllApiMappings(string swaggerText);
    }
}