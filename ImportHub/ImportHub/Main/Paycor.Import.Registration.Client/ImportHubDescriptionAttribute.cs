using System;

namespace Paycor.Import.Registration.Client
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class ImportHubDescriptionAttribute : Attribute
    {
        public ImportHubDescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }
}
