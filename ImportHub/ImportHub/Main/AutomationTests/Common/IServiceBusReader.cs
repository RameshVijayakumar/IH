
namespace Paycor.Import.ImportHubTest.Common
{
    public interface IServiceBusReader<T>
    {
        T ReceiveMessage();
    }
}
