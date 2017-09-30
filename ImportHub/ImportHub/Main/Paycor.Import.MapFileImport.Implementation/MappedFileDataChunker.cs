using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.LegacyShim;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation
{
    public class MappedFileDataChunker : IChunkData
    {
        private readonly ILog _logger;
        private readonly IMappedFileImportImporter _importer;

        public MappedFileDataChunker(ILog logger, IMappedFileImportImporter importer)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(importer, nameof(importer));
            _logger = logger;
            _importer = importer;
        }

        public ChunkDataResponse Create(ImportContext context, MappingDefinition map)
        {
            try
            {
                _logger.Debug($"Entered {nameof(Create)} from {nameof(MappedFileDataChunker)}.");

                var data = _importer.Import(context, map).ToList();
                TotalRecordCount = data.Count;
                FillContextImportHeaderInfo(context, TotalRecordCount);
                var chunks = BreakIntoChunks(data, context.ChunkSize);
                
                var enumerableChunks = chunks as IEnumerable<IDictionary<string, string>>[] ?? chunks.ToArray();
                return new ChunkDataResponse
                {
                    Status = Status.Success,
                    Chunks = enumerableChunks,
                    TotalChunks = enumerableChunks.Length,
                    TotalRecordsCount = TotalRecordCount
                };
            }
            catch (Exception exception)
            {
                var chunkerException = exception as ChunkerException;
                 
                _logger.Fatal("An exception occurred during the chunking process.", exception);
                var errorResultDataItems = new List<ErrorResultData>();
                var errorResultData = new ErrorResultData
                {
                    ErrorResponse = new ErrorResponse { Detail = "A problem occurred during the import process. please contact your Paycor Client Specialist for assistance." },
                    FailedRecord = null,
                    HttpExporterResult = null,
                    RowNumber = chunkerException?.RowFailure ?? 0
                };
                errorResultDataItems.Add(errorResultData);
                return new ChunkDataResponse
                {
                    Status = Status.Failure,
                    Error = exception,
                    ErrorResultDataItems = errorResultDataItems,
                    TotalRecordsCount = chunkerException?.TotalRows ?? 0
                };
            }
        }

        public int TotalRecordCount { get; set; }


        private IEnumerable<IEnumerable<IDictionary<string, string>>> BreakIntoChunks(IEnumerable<IDictionary<string, string>> data, int chunkSize)
        {
            if (data == null) return null;

            var chunks = new List<IEnumerable<IDictionary<string, string>>>();
            var items = data.ToList();

            if (items.Count <= 0)
            {
                return chunks;
            }

            var chunkCount = items.Count / chunkSize + (items.Count % chunkSize == 0 ? 0 : 1);
            _logger.Debug($"Total number of chunks is : {chunkCount}");
            var remainingRecords = items.Count;
            var retrievedRecords = 0;
            for (var chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {             
                var calculatedChunkSize = (remainingRecords >= chunkSize) ? chunkSize : remainingRecords;
                var chunk = items.Skip(retrievedRecords).Take(calculatedChunkSize);
                chunks.Add(chunk);
                remainingRecords = remainingRecords - chunkSize;
                retrievedRecords += calculatedChunkSize;
            }
            return chunks;
        }

        private static void FillContextImportHeaderInfo(ImportContext context, int totalRecordCount)
        {
            var objectType = context?.ImportHeaderInfo?.Keys.FirstOrDefault();
            if (objectType == null) return;

            var dictionary = new Dictionary<string, string> { [objectType] = totalRecordCount.ToString() };
            context.TotalRecordCount = totalRecordCount;
            context.ImportHeaderInfo = dictionary;
            context.TotalTabs = 1;
            context.DelayProcess = false;
        }
    }
}
