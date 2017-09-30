using System.Collections.Generic;

namespace Paycor.Import.MapFileImport.Implementation.Reporter
{
    public interface IProvideClientData<in T>
    {
        IEnumerable<string> GetAllClientIds(T input);
    }
}