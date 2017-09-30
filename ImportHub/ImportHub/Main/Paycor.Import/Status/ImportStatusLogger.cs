using System;
using System.Threading.Tasks;

//TODO: Missing unit tests

namespace Paycor.Import.Status
{
    public class ImportStatusLogger<T> where T : class
    {
        private readonly ImportStatusReceiver<T> _receiver;
        private readonly ImportStatusRetriever<T> _retriever;

        private readonly string _id;

        public ImportStatusLogger(
            string id,
            ImportStatusReceiver<T> receiver,
            IStatusStorageProvider storageProvider,
            StatusManager<T> manager,
            ImportStatusRetriever<T> retriever)
        {
            Ensure.ThatStringIsNotNullOrEmpty(id, nameof(id));
            Ensure.ThatArgumentIsNotNull(receiver, nameof(receiver));
            Ensure.ThatArgumentIsNotNull(retriever, nameof(retriever));

            _id = id;
            _receiver = receiver;
            _retriever = retriever;
        }

        public virtual void LogMessage(T message)
        {
            _receiver.Send(_id, message);
        }

        public virtual async Task LogMessageAsync(T message)
        {
            _receiver.Send(_id, message);
        }

        public virtual T RetrieveMessage()
        {
            var message = _retriever.RetrieveStatus(_id);

            if (message == null)
            {
                throw new ArgumentException(ImportStatusResource.StatusMessageNotFound);
            }
            return message;
        }
    }
}
