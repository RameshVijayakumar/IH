using Paycor.Import.Messaging;

namespace Paycor.Import.Azure
{
    public interface IWebJobProcessorFactory
    {
        IWebJobProcessor<FileUploadMessage> Create();
    }
}