namespace Paycor.Import.MapFileImport.Contract
{
    public interface ILookup
    {
        void Store(string key, string value);
        string Retrieve(string key);
        void Remove(string key);
    }
}
