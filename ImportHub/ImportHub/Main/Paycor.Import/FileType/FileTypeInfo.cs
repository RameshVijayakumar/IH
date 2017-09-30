using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Paycor.Import.FileType
{
    public abstract class FileTypeInfo : IFileTypeInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public FileTypeEnum FileType { get; set; }
    }
}
