using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Paycor.Import.Messaging
{
    [ExcludeFromCodeCoverage]
    public sealed class MappedImportFileTranslationData<T> : ImportFileTranslationData<T>
    {
        [DataMember]
        public Dictionary<HtmlVerb, string> Endpoints { get; set; }
        [DataMember]
        public bool CallApiInBatch { get; set; }

        [DataMember]
        public List<ApiFileTranslationData<T>> ApiFileTranslationData { get; set; }

    }
}