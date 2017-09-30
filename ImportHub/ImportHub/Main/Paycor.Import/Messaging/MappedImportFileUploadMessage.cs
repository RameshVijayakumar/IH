using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Paycor.Import.Mapping;

namespace Paycor.Import.Messaging
{
    public enum HtmlVerb
    {
        Post = 1,
        Put = 2,
        Delete = 4,
        Patch = 5,
        Get = 6,
        Upsert = 7
    }

    /// <summary>
    /// Message sent to service bus when a mapped file has been uploaded to the files controller in import hub.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class MappedImportFileUploadMessage : FileUploadMessage
    {
        [DataMember]
        public ApiMapping ApiMapping { get; set; }
    }
}