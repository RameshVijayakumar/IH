using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.MapFileImport.Implementation.Sender;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation
{
    public class MappedFileImportProcessOrchestrator : ProcessOrchestrator<MappedImportFileUploadMessage>
    {
        private const int DefaultChunkSize = 1;
        private readonly int _chunkSize;
        private readonly IChunkData _chunker;
        private readonly IDataSourceBuilder _builder;
        private readonly IPreparePayload _preparer;
        private readonly IReporter _reporter;
        private readonly IPayloadSenderFactory _senderFactory;
        
        public MappedFileImportProcessOrchestrator(ILog logger,
            IChunkData chunker,
            IDataSourceBuilder builder,
            IPreparePayload preparer,
            IPayloadSenderFactory senderFactory,
            IReporter reporter,
            IStoreData<ImportCancelToken> storeData) : base(logger, reporter, storeData)
        {
            Ensure.ThatArgumentIsNotNull(chunker, nameof(chunker));
            Ensure.ThatArgumentIsNotNull(builder, nameof(builder));
            Ensure.ThatArgumentIsNotNull(preparer, nameof(preparer));
            Ensure.ThatArgumentIsNotNull(reporter, nameof(reporter));
            Ensure.ThatArgumentIsNotNull(senderFactory, nameof(senderFactory));

            _chunker = chunker;
            _builder = builder;
            _preparer = preparer;
            _reporter = reporter;
            _senderFactory = senderFactory;
            _chunkSize = GetDefaultChunkSize();
        }

        public override async Task InnerProcessAsync(MappedImportFileUploadMessage message, ImportContext context)
        {
            var transactionId = message.TransactionId;
            if (IsImportCancelled(transactionId, nameof(StepNameEnum.Chunker), 0))
                return;

            var chunkerResponse = _chunker.Create(context, message.ApiMapping.Mapping);
            await _reporter.ReportAsync(StepNameEnum.Chunker, chunkerResponse);

            if (chunkerResponse.Status == Status.Failure || chunkerResponse.Chunks == null)
                return;

            foreach (var chunk in chunkerResponse.Chunks)
            {
                context.ChunkNumber++;
                SetLoggingContext(context);
                if (IsImportCancelled(transactionId, nameof(StepNameEnum.Builder), context.ChunkNumber))
                    break;

                var buildResponse = _builder.Build(context, message.ApiMapping, chunk);
                await _reporter.ReportAsync(StepNameEnum.Builder, buildResponse);
                if (buildResponse.Status == Status.Failure) break;
                if (IsImportCancelled(transactionId, nameof(StepNameEnum.Preparer), context.ChunkNumber))
                    break;

                var prepareResponse = _preparer.Prepare(context, message.ApiMapping,
                    buildResponse.DataSource);
                await _reporter.ReportAsync(StepNameEnum.Preparer, prepareResponse);
                if (prepareResponse.Status == Status.Failure) break;
                if (IsImportCancelled(transactionId, nameof(StepNameEnum.Sender), context.ChunkNumber))
                    break;

                var sender = _senderFactory.GetSenderExtracter(context.CallApiInBatch);
                var sendResponse = await sender.SendAsync(context, prepareResponse.PayloadDataItems);
                await _reporter.ReportAsync(StepNameEnum.Sender, sendResponse);
                if(sendResponse.Status == Status.Failure) break;
            }
        }



        protected override ImportContext CreateContext(MappedImportFileUploadMessage message)
        {
           
            return new ImportContext
            {
                ChunkNumber = 0,
                ChunkSize = CalculateChunkSize(message),
                FileName = message.File,
                MasterSessionId = message.MasterSessionId,
                TransactionId = message.TransactionId,
                CallApiInBatch = message.ApiMapping.IsBatchSupported,
                UploadedFileName = message.UploadedFileName,
                Endpoints = message.ApiMapping.MappingEndpoints.ToEndpointDictionary(),
                HasHeader = !message.ApiMapping.IsHeaderlessCustomMap(),
                Container = message.Container,
                ImportHeaderInfo = new Dictionary<string, string> { [message.ApiMapping.ObjectType] = "0"},
                ApiMapping = new List<ApiMapping> { message.ApiMapping}
                
            };
        }

        private int CalculateChunkSize(MappedImportFileUploadMessage message)
        {
            if (!message.ApiMapping.IsBatchSupported)
            {
                return message.ApiMapping.ChunkSize == 0 ? _chunkSize : message.ApiMapping.ChunkSize;
            }

            if (!message.ApiMapping.IsBatchChunkingSupported) return int.MaxValue;

            if (message.ApiMapping.PreferredBatchChunkSize == 0 || message.ApiMapping.PreferredBatchChunkSize == null)
            {
                return _chunkSize;
            }
            return (int)message.ApiMapping.PreferredBatchChunkSize;
        }

        private static int GetDefaultChunkSize()
        {
            int result;
            return int.TryParse(ConfigurationManager.AppSettings["ChunkSize"], out result) ? result : DefaultChunkSize;
        }
    }
}

