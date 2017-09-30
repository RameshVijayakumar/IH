using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Paycor.Import.Status
{
    [ExcludeFromCodeCoverage]
    public class ImportStatusReceiver<T> : BaseStatusReceiver<T>
    {
        public ImportStatusReceiver(string reporter) : base(reporter)
        {

        }

        protected override string Serialize(T statusType)
        {
            return JsonConvert.SerializeObject(statusType);
        }
    }
}