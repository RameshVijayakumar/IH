using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    //This interface will certify the maps and will return only valid maps which pass
    // the rules of certification.
    public interface IVerifyMaps
    {
        IEnumerable<GeneratedMapping> CertifyMaps(IEnumerable<GeneratedMapping> apiMappings);
    }
}
