using System.Collections.Generic;
using log4net;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;

namespace Paycor.Import.MapFileImport.Implementation.Sender
{
    public class SendPayloadFactory : BasePayloadSenderFactory
    {
        private readonly ILog _logger;
        private readonly IHttpInvoker _httpInvoker;
        private readonly IGenerateFailedRecord _generateFailedRecord;
        private readonly IApiExecutor _apiExecutor;

        public SendPayloadFactory(ILog logger, IHttpInvoker httpInvoker, IGenerateFailedRecord generateFailedRecord,
            IApiExecutor apiExecutor)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(httpInvoker, nameof(httpInvoker));
            Ensure.ThatArgumentIsNotNull(generateFailedRecord, nameof(generateFailedRecord));
            Ensure.ThatArgumentIsNotNull(apiExecutor, nameof(apiExecutor));

            _logger = logger;
            _httpInvoker = httpInvoker;
            _generateFailedRecord = generateFailedRecord;
            _apiExecutor = apiExecutor;
            LoadHandlers();
        }

        public sealed override void LoadHandlers()
        {
           AddSendPayload(
           new List<IPayloadSender>
           {
                new PayloadSender(_logger, _apiExecutor),
                new BatchPayloadSender(_logger,_httpInvoker,_generateFailedRecord)
           });
        }
    }
}
