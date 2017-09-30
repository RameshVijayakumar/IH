using System;
using System.Diagnostics.CodeAnalysis;

namespace Paycor.Import.Registration.Client
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnorePropertyAttribute : Attribute
    {
        // This is basically just a marker class with no behavior
    }
}
