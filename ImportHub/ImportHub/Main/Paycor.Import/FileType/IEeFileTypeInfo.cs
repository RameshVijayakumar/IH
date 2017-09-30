namespace Paycor.Import.FileType
{
    public interface IEeFileTypeInfo : IFileTypeInfo
    {
        string MappingValue { get; set; }
    }
}