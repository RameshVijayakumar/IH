using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation
{
    public class MappedFileDataSourceBuilder : IDataSourceBuilder
    {
        private readonly ILog _logger;
        private readonly IEnumerable<ITransformRecordFields<MappingDefinition>> _fieldTransformers;
        private readonly ITransformAliasRecordFields<MappingDefinition> _transformRecordFields;
        private readonly IRecordSplitter<MappingDefinition> _recordSplitter;
        private List<ErrorResultData> _errorResultDataItems;
        private readonly ILookup _lookup;

        public MappedFileDataSourceBuilder(ILog logger, IEnumerable<ITransformRecordFields<MappingDefinition>> fieldTransformers,
            ITransformAliasRecordFields<MappingDefinition> transformRecordFields, 
            IRecordSplitter<MappingDefinition> recordSplitter, ILookup lookup)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(fieldTransformers, nameof(fieldTransformers));
            Ensure.ThatArgumentIsNotNull(transformRecordFields, nameof(transformRecordFields));
            Ensure.ThatArgumentIsNotNull(recordSplitter, nameof(recordSplitter));
            Ensure.ThatArgumentIsNotNull(lookup, nameof(lookup));

            _logger = logger;
            _transformRecordFields = transformRecordFields;
            _fieldTransformers = fieldTransformers;
            _recordSplitter = recordSplitter;
            _lookup = lookup;
        }

        public BuildDataSourceResponse Build(ImportContext context, ApiMapping mapping, 
            IEnumerable<IDictionary<string, string>> chunk)
        {
            Ensure.ThatArgumentIsNotNull(context, nameof(context));
            Ensure.ThatArgumentIsNotNull(mapping, nameof(mapping));
            Ensure.ThatArgumentIsNotNull(chunk, nameof(chunk));

            try
            {
                _logger.Info($"{nameof(MappedFileDataSourceBuilder)} entered.");
                _errorResultDataItems = new List<ErrorResultData>();
                var chunkedDataSources = chunk.Select(item => TransformChunkRecord(mapping.Mapping, item, context.MasterSessionId)).ToList();
                var aliasChunkedDataSources = _transformRecordFields.TransformAliasRecordFields(mapping.Mapping, chunkedDataSources, context.MasterSessionId);
                var finalChunkedDataSources = aliasChunkedDataSources as IEnumerable<KeyValuePair<string, string>>[] ?? aliasChunkedDataSources.ToArray();
                var finalDataSources = _recordSplitter.TransformRecordsToDictionaryList(mapping.Mapping, finalChunkedDataSources);
                
                var mappedChunkDataSource = new MappedFileChunkDataSource
                {
                    Records = finalDataSources
                };

                return new BuildDataSourceResponse
                {
                    DataSource = mappedChunkDataSource,
                    PayloadCount = mappedChunkDataSource.Records.Count(),
                    ImportType =  mapping.MappingName
                };
            }
            catch (Exception exception)
            {
                _logger.Fatal("An exception occurred during the build process.", exception);
                var errorResultData = new ErrorResultData
                {
                    ErrorResponse = new ErrorResponse { Detail = "A problem occurred during the import process, please contact Paycor Specialist" },
                    FailedRecord = null,
                    HttpExporterResult = null,
                    RowNumber = 0,
                    ImportType = mapping.MappingName
                };
                _errorResultDataItems.Add(errorResultData);
                return new BuildDataSourceResponse
                {
                    Error = exception,
                    Status = Status.Failure,
                    ErrorResultDataItems = _errorResultDataItems
                };
            }
        }

        private IEnumerable<KeyValuePair<string,string>> TransformChunkRecord(MappingDefinition mappingDefinition, 
            IDictionary<string, string> record, string masterSessionId)
        {
            List<KeyValuePair<string, string>> kvpRecord = null;

            foreach (var recordKvp in _fieldTransformers.Select(fieldtransformer => fieldtransformer.TransformRecordFields(mappingDefinition, masterSessionId, record, kvpRecord,_lookup)))
            {
                record = null;
                kvpRecord = recordKvp.ToList();
            }
            return kvpRecord;
       } 
    }
}
