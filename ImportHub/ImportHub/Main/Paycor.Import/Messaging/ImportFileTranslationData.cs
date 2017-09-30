using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Paycor.Import.Messaging
{
    [ExcludeFromCodeCoverage]
    public abstract class ImportFileTranslationData<T> : FileTranslationData<T>
    {
        [DataMember]
        public string TransactionId { get; set; }

        [DataMember]
        public string MasterSessionId { get; set; }
    }
}