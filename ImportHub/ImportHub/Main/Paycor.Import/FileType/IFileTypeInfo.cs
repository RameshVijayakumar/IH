using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.FileType
{
    public interface IFileTypeInfo
    {
        FileTypeEnum FileType { get; set; }
    }
}