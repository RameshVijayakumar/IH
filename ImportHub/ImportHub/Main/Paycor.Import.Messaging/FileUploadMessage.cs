
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Paycor.Import.Messaging
{
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class FileUploadMessage
    {
        [DataMember]
        public string Container { get; set; }

        [DataMember]
        public string File { get; set; }
        [DataMember]
        public string TransactionId { get; set; }

        [DataMember]
        public string MasterSessionId { get; set; }

        [DataMember]
        public string UploadedFileName { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}