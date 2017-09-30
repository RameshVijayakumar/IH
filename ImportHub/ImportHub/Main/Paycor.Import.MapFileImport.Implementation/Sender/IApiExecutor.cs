using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation.Sender
{
    public interface IApiExecutor
    {
        Task<ApiResult> ExecuteApiAsync(Guid masterSessionId, PayloadData payloadDataItem,
            IDictionary<string, string> headerData);
    }
}