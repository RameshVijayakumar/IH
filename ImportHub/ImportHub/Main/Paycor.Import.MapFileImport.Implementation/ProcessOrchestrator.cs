using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation
{
    public abstract class ProcessOrchestrator<TMsg> where TMsg : FileUploadMessage
    {
        private readonly ILog _logger;
        private readonly IReporter _reporter;
        private readonly IStoreData<ImportCancelToken> _storeData;

        protected ProcessOrchestrator(ILog logger,
            IReporter reporter, IStoreData<ImportCancelToken> storeData)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(reporter, nameof(reporter));
            Ensure.ThatArgumentIsNotNull(storeData, nameof(storeData));
            _logger = logger;
            _reporter = reporter;
            _storeData = storeData;
        }

        public virtual async Task ProcessAsync(TMsg message)
        {
            _logger.Debug($"{nameof(Process)} started with message: {message.MasterSessionId}, file: {message.UploadedFileName}.");

            var context = CreateContext(message);

            _logger.Debug(context);
            SetLoggingContext(context);
            _reporter.Initialize(context);

            try
            {
                _logger.Info($"Context chunk size set to {context.ChunkSize}.");
                await InnerProcessAsync(message, context);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    "ImportHub has received an unhandled exception within the processor; however, this should not occur. Please investigate by reviewing the exception stack trace.",
                    ex);
            }
            finally
            {
                try
                {
                    await _reporter.ReportCompletionAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error($"An error occurred while trying to report the completion of import transaction {message.TransactionId}.", ex);
                }
                _logger.Info($"Import transaction {message.TransactionId} has completed all processing.");
                _logger.Debug($"{nameof(Process)} completed.");
            }
        }

        public abstract Task InnerProcessAsync(TMsg message, ImportContext context);

        protected abstract ImportContext CreateContext(TMsg message);

        protected static void SetLoggingContext(ImportContext context)
        {
            LogicalThreadContext.Properties["ChunkNumber"] = context.ChunkNumber;
            LogicalThreadContext.Properties["MasterSessionId"] = context.MasterSessionId;
            LogicalThreadContext.Properties["TransactionId"] = context.TransactionId;
            LogicalThreadContext.Properties["FileName"] = context.UploadedFileName;
        }

        protected bool IsImportCancelled(string transactionId, string step, int chunkNumber)
        {
            var importCancelToken = _storeData.Retrieve(transactionId);
            if (importCancelToken == null || !importCancelToken.CancelRequested) return false;

            _logger.Info($"Import cancelled at chunk number {chunkNumber} before {step} process");
            _reporter.CanceledReport();
            return true;
        }
    }
}
