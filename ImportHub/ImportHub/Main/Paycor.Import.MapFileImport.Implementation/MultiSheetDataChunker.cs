using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.LegacyShim;
using Paycor.Import.Mapping;
// ReSharper disable All

namespace Paycor.Import.MapFileImport.Implementation
{
    public class MultiSheetDataChunker : IChunkMultiData
    {
        private readonly ILog _logger;
        private readonly IMultiSheetImportImporter _importer;

        public MultiSheetDataChunker(ILog logger, IMultiSheetImportImporter importer)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(importer, nameof(importer));
            _logger = logger;
            _importer = importer;
        }

        public ChunkMultiDataResponse Create(ImportContext context, IList<ApiMapping> mappings)
        {
            try
            {
                _logger.Debug($"Entered {nameof(Create)} from {nameof(MultiSheetDataChunker)}." );
                var sheetData = _importer.Import(context, mappings);                

                SetContextImportHeaderInfo(sheetData, context);
                var multiSheetChunks = BreakEachTabDataIntoChunks(sheetData, context.ChunkSize);
                return new ChunkMultiDataResponse
                {
                    Status = Status.Success,
                    Chunks = sheetData,
                    TotalChunks = multiSheetChunks.Count(),
                    TotalRecordsCount = TotalRecordCount,
                    MultiSheetChunks = multiSheetChunks
                };
            }
            catch (Exception exception)
            {
                var chunkerException = exception as ChunkerException;
                _logger.Fatal("An exception occurred during the chunking process.", exception);

                var errorResultDataItems = new List<ErrorResultData>();
                var errorResultData = new ErrorResultData
                {
                    ErrorResponse = new ErrorResponse { Detail = exception.Message },
                    FailedRecord = null,
                    HttpExporterResult = null,
                    RowNumber = chunkerException?.RowFailure ?? 0,
                    ImportType = chunkerException?.ImportType
                };
                errorResultDataItems.Add(errorResultData);
                return new ChunkMultiDataResponse
                {
                    Status = Status.Failure,
                    Error = exception,
                    ErrorResultDataItems = errorResultDataItems,
                    TotalRecordsCount = chunkerException?.TotalRows ?? 0   
                };
            }
        }

        private void SetContextImportHeaderInfo(IEnumerable<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>> sheetdata,
            ImportContext context)
        {
            var dict = new Dictionary<string, string>();
            foreach (var data in sheetdata)
            {
                if (data.Value != null)
                {
                    TotalRecordCount += data.Value.Count;
                    dict[data.Key.ObjectType] = data.Value.Count.ToString();
                    if (data.Key.ObjectType.RemoveWhiteSpaces()
                            .Equals(ImportConstants.EmployeeTaxImport.RemoveWhiteSpaces(), StringComparison.OrdinalIgnoreCase) ||
                        data.Key.ObjectType.RemoveWhiteSpaces()
                            .Equals(ImportConstants.EmployeeEarningImport.RemoveWhiteSpaces(), StringComparison.OrdinalIgnoreCase))
                    {
                        context.DelayProcess = true;
                    }
                }
            }
            context.TotalTabs = sheetdata.Count();
            context.TotalRecordCount = TotalRecordCount;
            context.ImportHeaderInfo = dict;
        }



        public int TotalRecordCount { get; set; }

        private IEnumerable<SheetChunk> BreakEachTabDataIntoChunks(IList<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>> sheetData, int defaultChunkSize)
        {
            var multiSheetChunks = new List<SheetChunk>();
            foreach (var data in sheetData)
            {
                var items = data.Value.ToList();
                var chunkSize = data.Key.ChunkSize == 0 ? defaultChunkSize : data.Key.ChunkSize;
                var chunkCount = items.Count / chunkSize + (items.Count % chunkSize == 0 ? 0 : 1);
                _logger.Info($"Total number of chunks is : {chunkCount} for tab data {data.Key?.MappingName} taking chunkSize of {chunkSize}");
                var remainingRecords = items.Count;
                var retrievedRecords = 0;
                for (var chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
                {                   
                    var chunkData = new List<IDictionary<string, string>>();
                    var calculatedChunkSize = (remainingRecords >= chunkSize) ? chunkSize : remainingRecords;
                    var chunk = items.Skip(retrievedRecords).Take(calculatedChunkSize);
                    chunkData.AddRange(chunk);
                    var multiSheetChunk = new SheetChunk
                    {
                        ChunkTabData = chunkData,
                        ApiMapping = data.Key
                    };
                    remainingRecords = remainingRecords - chunkSize;
                    retrievedRecords += calculatedChunkSize;
                    multiSheetChunks.Add(multiSheetChunk);
                }
            }
            return multiSheetChunks;
        }
        
    }
}
