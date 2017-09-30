using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation.Sender
{
    public interface IPayloadSenderFactory
    {
        void LoadHandlers();
        IPayloadSender GetSenderExtracter(bool isBatch);
        
    }
}