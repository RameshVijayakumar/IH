using System;

namespace Paycor.Import.Registration.Client
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BatchChunkingSupportAttribute : Attribute
    {
        public int? PreferredChunkSize { get; private set; }

        public BatchChunkingSupportAttribute()
        {
            PreferredChunkSize = null;
        }

        public BatchChunkingSupportAttribute(int preferredChunkSize)
        {
            PreferredChunkSize = preferredChunkSize;
        }
    }
}
