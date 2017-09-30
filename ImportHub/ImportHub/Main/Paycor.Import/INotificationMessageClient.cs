using System;

namespace Paycor.Import
{
    public interface INotificationMessageClient
    {
        void Send(Guid userIdentity, string apiKey, string apiSecretKey, string purpose, string shortMessage, string mediumMessage, string longMessage, string notificationType);
    }
}
