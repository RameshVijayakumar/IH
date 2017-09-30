using System;

namespace Paycor.Import.Registration.Client
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class ChunkSizeAttribute : Attribute
    {
        public ChunkSizeAttribute(int chunkSize = 1)
        {
            ChunkSize = chunkSize;
        }
        public int ChunkSize { get; private set; }
    }
}    