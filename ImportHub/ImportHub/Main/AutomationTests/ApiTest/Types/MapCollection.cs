using System.Collections.Generic;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class MapCollection 
    {
        public IEnumerable<Map> Maps { get; set; }
        public override string ToString()
        {
            return this.Serialize();
        }
    }
}
