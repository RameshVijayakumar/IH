using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Paycor.Import.Messaging
{
    /// <summary>
    /// Message sent to service bus when a classic EE import file is uploaded to the files controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class EeImportFileUploadMessage : FileUploadMessage
    {
        [DataMember]
        public string MappingValue { get; set; }
    }
}