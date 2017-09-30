
using System.Collections.Generic;
using System.Runtime.Serialization;
using Paycor.Import.Mapping;

namespace Paycor.Import.Messaging
{
    public class MultiSheetImportStatusMessage : FileUploadMessage
    {
        [DataMember]
        public IEnumerable<ApiMapping> BaseMappings { get; set; }
    }

}
