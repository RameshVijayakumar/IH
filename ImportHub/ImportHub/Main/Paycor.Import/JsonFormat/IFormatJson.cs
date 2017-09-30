using System.Collections.Generic;

namespace Paycor.Import.JsonFormat
{
    public interface IFormatJson
    {
        string RemoveProperties(string data, List<string> removeList);
    }
}