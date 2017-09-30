using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.Messaging;
using Paycor.Messaging.Contract.v1;
using Paycor.Security.Principal;

namespace Paycor.Import
{
    public class MockNotificationMessageClient : INotificationMessageClient
    {
        private class Recipient : IRecipient
        {
            [JsonProperty("userKey")]
            public string UserKey { get; set; }
        }

        private class Message : IMessage<Recipient>
        {
            [JsonProperty("recipients")]
            public IEnumerable<Recipient> Recipients { get; set; }

            [JsonProperty("purpose")]
            public string Purpose { get; set; }

            [JsonProperty("shortMessage")]
            public string ShortMessage { get; set; }

            [JsonProperty("mediumMessage")]
            public string MediumMessage { get; set; }

            [JsonProperty("longMessage")]
            public string LongMessage { get; set; }

            [JsonProperty("messageTypeId")]
            public string MessageTypeId { get; set; }

            [JsonProperty("metaData")]
            public IDictionary<string, string> MetaData { get; set; }

            [JsonProperty("isSecure")]
            public bool? IsSecure { get; set; }
        }

        private readonly ILog _log;
        private readonly IHttpInvoker _httpInvoker;
        private readonly string _messagingEndpoint;

        public MockNotificationMessageClient(ILog log, IHttpInvoker httpInvoker, string messagingEndpoint)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(httpInvoker, nameof(httpInvoker));
            Ensure.ThatStringIsNotNullOrEmpty(messagingEndpoint, nameof(messagingEndpoint));
            _log = log;
            _httpInvoker = httpInvoker;
            _messagingEndpoint = messagingEndpoint.Replace("https","http");
        }

        public void Send(Guid masterSessionId, string apiKey, string apiSecretKey,string purpose, string shortMessage, string mediumMessage, 
            string longMessage, string notificationType)
        {
            _log.Debug($"Sending Notification Message: purpose: {purpose}, type: {notificationType}");
            var puf = new PaycorUserFactory();
            var user = puf.GetPrincipal(masterSessionId);
            var userKey = user.UserKey;
            var recipients = new List<Recipient>
            {
                new Recipient()
                {
                    UserKey = userKey.ToString()
                }
            };

            var newMessage = new Message
            {
                LongMessage = longMessage,
                ShortMessage = shortMessage,
                MediumMessage = mediumMessage,
                Purpose = purpose,
                MessageTypeId = notificationType,
                Recipients = recipients
            };
            var messagePayload = JsonConvert.SerializeObject(newMessage);
            _log.Debug($"Debug notification message: {messagePayload}");

            // send the message to the person who started the import
            var response = _httpInvoker.CallApiEndpoint(masterSessionId, messagePayload, _messagingEndpoint, HtmlVerb.Post);
            _log.Debug($"Response received: result: {response.Result}, is success? {response.IsSuccess}");
            if (response.IsSuccess) return;

            _log.Warn("An attempt to notify the user that their import completed has failed.", response.Exception);
            var errResponse = response.GetErrorResponse(_log)?.ToString();
            if (!string.IsNullOrEmpty(errResponse))
            {
                _log.Warn(errResponse);
            }
        }
    }
}
