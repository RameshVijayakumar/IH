using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paycor.Import.Registration
{
    public interface IMappingCertification
    {
        bool IsAllMappingCertified(string swaggerText);
    }
}
