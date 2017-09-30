using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Paycor.Import.Messaging
{
    [ExcludeFromCodeCoverage]
    public sealed class EeImportFileTranslationData<T> : ImportFileTranslationData<T>
    {
        [DataMember]
        public string MappingValue { get; set; }
    }
}