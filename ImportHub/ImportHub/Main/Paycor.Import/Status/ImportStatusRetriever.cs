using Newtonsoft.Json;

namespace Paycor.Import.Status
{
    public class ImportStatusRetriever<T> : BaseStatusRetriever<T>
    {
        public ImportStatusRetriever(IStatusStorageProvider provider, string reporter) : base(provider, reporter)
        {
        }

        protected override T Deserialize(StatusMessage message)
        {
            var statusMessage = default(T);
            if (message != null)
            {
                statusMessage = JsonConvert.DeserializeObject<T>(message.Status);
            }
            return statusMessage;
        }
    }
}