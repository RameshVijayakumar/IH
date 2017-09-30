using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation.Sender
{
    public abstract class BasePayloadSenderFactory: IPayloadSenderFactory
    {
        private readonly List<IPayloadSender> _payloadSenders = new List<IPayloadSender>();
        public abstract void LoadHandlers();

        public IPayloadSender GetSenderExtracter(bool isBatch)
        {
           return _payloadSenders.Single(t=>t.GetPayloadSenderType() == isBatch);
        }

        protected void AddSendPayload(IEnumerable<IPayloadSender> sendPayloadSenders)
        {
            _payloadSenders.AddRange(sendPayloadSenders);
        }
    }
}
