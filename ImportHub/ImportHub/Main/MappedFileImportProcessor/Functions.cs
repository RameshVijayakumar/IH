using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Microsoft.Azure.WebJobs;
using Ninject;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.MapFileImport.Implementation.Sender;
using Paycor.Import.Messaging;


namespace MappedFileImportProcessor
{
    public class Functions
    {
        public static async Task ProcessSingleSheetAsync(
            [ServiceBusTrigger(QueueNames.MappedFileImport)] MappedImportFileUploadMessage message)
        {
            var kernel = KernelFactory.GetKernel();
            var logger = kernel.Get<ILog>();
            var reporter = kernel.Get<IReporter>();

            try
            {
                logger.Info($"A single sheet import message was received with transaction Id: {message.TransactionId} and with map {message.ApiMapping?.MappingName}");
                logger.Debug($"{nameof(ProcessSingleSheetAsync)} entered.");
                var chunker = kernel.Get<IChunkData>();
                var builder = kernel.Get<IDataSourceBuilder>();
                var preparer = kernel.Get<IPreparePayload>();
                var storeData = kernel.Get<IStoreData<ImportCancelToken>>();
                var senderFactory = kernel.Get<IPayloadSenderFactory>();
                var orchestrator = new MappedFileImportProcessOrchestrator(logger, chunker, builder, preparer, senderFactory,
                    reporter, storeData);

                await orchestrator.ProcessAsync(message);
                logger.Debug($"{nameof(ProcessSingleSheetAsync)} exited.");
            }
            catch (Exception ex)
            {
                try
                {
                    logger.Fatal(
                   $"An un expected error occurred during the processing of single sheet import transaction: {message.TransactionId}.",
                   ex);
                    await CloseOutReportingAsync(reporter, ex);
                }
                catch (Exception rptEx)
                {
                    try
                    {
                        logger.Fatal(
                            $"There was a problem trying to report the completion for transaction {message.TransactionId}.",
                            rptEx);
                    }
                    catch (Exception)
                    {
                        //Eat Exception to avoid re-processing
                    }
                }
            }
        }

        public static async Task ProcessMultiSheetAsync(
            [ServiceBusTrigger(QueueNames.MultiFileImport)] MultiSheetImportStatusMessage message)
        {
            var kernel = KernelFactory.GetKernel();
            var logger = kernel.Get<ILog>();
            var reporter = kernel.Get<IReporter>();
            try
            {
                logger.Info($"A multi sheet import message was received with transaction Id: {message.TransactionId}.");
                logger.Debug($"{nameof(ProcessMultiSheetAsync)} entered.");
                var chunker = kernel.Get<IChunkMultiData>();
                var builder = kernel.Get<IDataSourceBuilder>();
                var preparer = kernel.Get<IPreparePayload>();
                var storeData = kernel.Get<IStoreData<ImportCancelToken>>();
                var senderFactory = kernel.Get<IPayloadSenderFactory>();
                var orchestrator = new MultiSheetImportProcessOrchestrator(logger, chunker, builder, preparer, senderFactory,
                    reporter, storeData);

                await orchestrator.ProcessAsync(message);
                logger.Debug($"{nameof(ProcessMultiSheetAsync)} exited.");
            }
            catch (Exception ex)
            {
                try
                {
                    logger.Fatal($"An un expected error occurred during the processing of multi sheet import transaction: {message.TransactionId}.", ex);
                    await CloseOutReportingAsync(reporter, ex);
                }
                catch (Exception rptEx)
                {
                    try
                    {
                        logger.Fatal(
                            $"There was a problem trying to report the completion for transaction {message.TransactionId}.",
                            rptEx);
                    }
                    catch (Exception)
                    {
                        //Eat Exception to avoid re-processing
                    }
                }
            }
        }

        private static async Task CloseOutReportingAsync(IReporter reporter, Exception ex)
        {
            var errorResultDataItems = new List<ErrorResultData>();
            var errorResultData = new ErrorResultData
            {
                ErrorResponse = new ErrorResponse { Detail = "A problem occurred during the import process. please contact your Paycor Client Specialist for assistance." },
                FailedRecord = null,
                HttpExporterResult = null,
            };
            errorResultDataItems.Add(errorResultData);
            await reporter.ReportAsync(StepNameEnum.Chunker, new ChunkDataResponse
            {
                Error = ex,
                ErrorResultDataItems = errorResultDataItems,
                Status = Status.Failure,
            });
            await reporter.ReportCompletionAsync();
        }
    }
}
