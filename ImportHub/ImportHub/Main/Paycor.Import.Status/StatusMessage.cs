using System.Diagnostics.CodeAnalysis;

namespace Paycor.Import.Status
{
    [ExcludeFromCodeCoverage]
    public class StatusMessage
    {
        public string Reporter { get; set; }
        public string Key { get; set; }
        public string Status { get; set; }
    }
}
