using System.Diagnostics.CodeAnalysis;

namespace Paycor.Import.Registration.Client
{
    [ExcludeFromCodeCoverage]
    public class RegistrationServiceTopicInfo
    {
        public const string TopicName = "paycorserviceregistration";
        public const string SubscriptionName = "importhub";
        public const string ServiceBusConnectionKey = "ServiceBusConnection";
        public const string SwaggerDocVersionKey = "SwaggerDocVersions";
    }
}