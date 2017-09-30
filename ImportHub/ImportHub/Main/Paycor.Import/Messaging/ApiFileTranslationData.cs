using System.Collections.Generic;

namespace Paycor.Import.Messaging
{
    public class ApiFileTranslationData<T>
    {
        public T Record { get; set; }
        public List<ApiFileArray<T>> ApiPayloadArrays { get; set; }
    }
}