using System.Diagnostics.CodeAnalysis;

namespace Paycor.Import.Azure
{
    [ExcludeFromCodeCoverage]
    public class MappedFileTopicInfo
    {
        public const string TopicName = "importhubevents";
        public const string ServiceBusConnectionKey = "PaycorServiceBusConnection";
        public const string EnvironmentCheckName = "Mapped File Import Event Topic";
    }
}
