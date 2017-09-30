using System.Collections.Generic;
using Paycor.Import.ImportHistory;

namespace Paycor.Import.Employee.ImportHistory
{
    public interface ILegacyCleanUp
    {
        void UpdateStuckLegacyHistoryMessages(IEnumerable<ImportHistoryMessage> historyMessages);
    }
}