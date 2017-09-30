namespace Paycor.Import.Messaging
{
    public interface IStoreData<T>
    {
        T Retrieve(string rowKey);

        void Store(T item);
    }
}
