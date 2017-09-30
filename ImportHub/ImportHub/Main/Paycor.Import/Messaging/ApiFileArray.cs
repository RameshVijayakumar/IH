using System.Collections.Generic;

namespace Paycor.Import.Messaging
{
    public class ApiFileArray<T>
    {
        public string ArrayName { get; set; }
        public IEnumerable<T> ArrayData { get; set; }
    }
}