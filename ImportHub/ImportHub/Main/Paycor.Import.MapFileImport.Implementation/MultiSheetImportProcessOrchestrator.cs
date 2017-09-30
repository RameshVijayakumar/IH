using System.Linq;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.MapFileImport.Implementation.Sender;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation
{
    public class MultiSheetImportProcessOrchestrator : ProcessOrchestrator<MultiSheetImportStatusMessage>
    {
        private const int DefaultChunkSize = int.MaxValue;
        private readonly IChunkMultiData _chunker;
        private readonly IDataSourceBuilder _builder;
        private readonly IPreparePayload _preparer;
        private readonly IPayloadSenderFactory _senderFactory;
        private readonly IReporter _reporter;

        public MultiSheetImportProcessOrchestrator(ILog logger,
            IChunkMultiData chunker,
            IDataSourceBuilder builder,
            IPreparePayload preparer,
            IPayloadSenderFactory senderFactory,
            IReporter reporter,IStoreData<ImportCancelToken> storeData): base(logger, reporter, storeData)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
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
        }

        public override async Task InnerProcessAsync(MultiSheetImportStatusMessage message, ImportContext context)
        {
            var transactionId = message.TransactionId;
            if (IsImportCancelled(transactionId, nameof(StepNameEnum.Chunker), 0))
                return;

            var chunkerResponse = _chunker.Create(context, message.BaseMappings.ToList());
            await _reporter.ReportAsync(StepNameEnum.Chunker, chunkerResponse);

            if (chunkerResponse.Status == Status.Failure || chunkerResponse.Chunks == null) return;

            foreach (var multiSheetChunk in chunkerResponse.MultiSheetChunks)
            {
                context.ChunkNumber++;
                SetLoggingContext(context);

                context.Endpoints = multiSheetChunk.ApiMapping.MappingEndpoints.ToEndpointDictionary();
                context.CallApiInBatch = multiSheetChunk.ApiMapping.IsBatchSupported;
                if (IsImportCancelled(transactionId, nameof(StepNameEnum.Builder), context.ChunkNumber))
                    break;

                var buildResponse = _builder.Build(context, multiSheetChunk.ApiMapping, multiSheetChunk.ChunkTabData);
                await _reporter.ReportAsync(StepNameEnum.Builder, buildResponse);
                if (buildResponse.Status == Status.Failure) break;
                if (IsImportCancelled(transactionId, nameof(StepNameEnum.Preparer), context.ChunkNumber))
                    break;

                var prepareResponse = _preparer.Prepare(context, multiSheetChunk.ApiMapping,
                    buildResponse.DataSource);
                await _reporter.ReportAsync(StepNameEnum.Preparer, prepareResponse);
                if (prepareResponse.Status == Status.Failure) break;
                if (IsImportCancelled(transactionId, nameof(StepNameEnum.Sender), context.ChunkNumber))
                    break;

                var sender = _senderFactory.GetSenderExtracter(context.CallApiInBatch);
                var sendResponse = await sender.SendAsync(context, prepareResponse.PayloadDataItems);
                await _reporter.ReportAsync(StepNameEnum.Sender, sendResponse);
            }
        }


        protected override ImportContext CreateContext(MultiSheetImportStatusMessage message)
        {
            return new ImportContext
            {
                ChunkNumber = 0,
                ChunkSize = DefaultChunkSize,
                FileName = message.File,
                MasterSessionId = message.MasterSessionId,
                TransactionId = message.TransactionId,
                UploadedFileName = message.UploadedFileName,
                Container = message.Container,
                IsMultiSheetImport = true,
                ApiMapping = message.BaseMappings.ToList()
            };
        }
    }
}

