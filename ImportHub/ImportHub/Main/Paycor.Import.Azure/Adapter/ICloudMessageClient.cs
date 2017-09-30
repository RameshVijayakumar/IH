namespace Paycor.Import.Azure.Adapter
{
    public interface ICloudMessageClient<in T>
    {
        void SendMessage(T message, string queue, string serviceBusConnectionString);
    }
}