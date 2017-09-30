using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Paycor.Import.Messaging
{
    [ExcludeFromCodeCoverage]
    public sealed class EeImportRestApiPayload : RestApiPayload
    {
        [DataMember]
        public string MappingValue { get; set; }
    }
}