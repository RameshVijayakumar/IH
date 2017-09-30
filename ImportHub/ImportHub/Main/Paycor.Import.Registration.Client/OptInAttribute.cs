using System;

namespace Paycor.Import.Registration.Client
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OptInAttribute : Attribute
    {
        public OptInAttribute(bool optIn = true)
        {
            OptIn = optIn;
        }
        public bool OptIn { get; private set; }
    }
}
