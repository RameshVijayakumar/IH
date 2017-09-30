using System;

namespace Paycor.Import.Registration.Client
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    public sealed class LookupRouteAttribute : Attribute
    {
        public string Route { get; set; }

        public string ValuePath { get; set; }

        public string Property { get; set; }

        public string ExceptionMessage { get; set; }

        public bool IsRequiredForPayload { get; set; }

        public LookupRouteAttribute(string route, string property, string exceptionMessage,  string valuePath = null, bool isRequiredForPayload = false)
        {
            Route = route;
            Property = property;
            ValuePath = valuePath;
            ExceptionMessage = exceptionMessage;
            IsRequiredForPayload = isRequiredForPayload;
        }
    }
}
