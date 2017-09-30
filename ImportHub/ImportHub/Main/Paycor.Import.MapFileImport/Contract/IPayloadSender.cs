using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paycor.Import.MapFileImport.Contract
{
    public interface IPayloadSender
    {
        Task<PayloadSenderResponse> SendAsync(ImportContext context, IEnumerable<PayloadData> payloadDataItems);
        bool GetPayloadSenderType();
    }
}
