using System;
using System.Collections.Generic;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public class MappedFilePayloadPreparer : IPreparePayload
    {
        private readonly ILog _logger;
        private readonly IFlatToApiRecordsTransformer _flatToApiRecordsTransformer;
        private readonly IPreparePayloadFactory _preparePayloadFactory;
        private List<ErrorResultData> _errorResultDataItems;


        public MappedFilePayloadPreparer(ILog logger, IFlatToApiRecordsTransformer flatToApiRecordsTransformer,
            IPreparePayloadFactory preparePayloadFactory)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(flatToApiRecordsTransformer, nameof(flatToApiRecordsTransformer));
            Ensure.ThatArgumentIsNotNull(preparePayloadFactory, nameof(preparePayloadFactory));


            _logger = logger;
            _flatToApiRecordsTransformer = flatToApiRecordsTransformer;
            _preparePayloadFactory = preparePayloadFactory;
            _preparePayloadFactory.LoadHandlers();
        }

        public PreparePayloadResponse Prepare(ImportContext context, ApiMapping mapping, IChunkDataSource dataSource)
        {
            try
            {
                _logger.Info($"{nameof(MappedFilePayloadPreparer)} entered.");
                _errorResultDataItems = new List<ErrorResultData>();

                var apiRecords = _flatToApiRecordsTransformer.TranslateFlatRecordsToApiRecords(dataSource.Records, mapping, context);
                var preparePayloadResponse = new PreparePayloadResponse();

                var payloadType = context.CallApiInBatch ? PreparePayloadTypeEnum.Batch : PreparePayloadTypeEnum.NonBatch;
                var payloadPreparer = _preparePayloadFactory.GetPayloadExtracter(payloadType);

                var payloadDataItems = payloadPreparer.PreparePayload(context, apiRecords);
                var errorResultItems = payloadPreparer.GetErrorResultItems();
                
                preparePayloadResponse.PayloadDataItems = payloadDataItems;
                preparePayloadResponse.ErrorResultDataItems = errorResultItems;
               
                return preparePayloadResponse;
            }
            catch (Exception exception)
            {
                _logger.Fatal(exception.ToString());
                var errorResultData = new ErrorResultData
                {
                    ErrorResponse = new ErrorResponse { Detail = "A problem occurred during the import process, please contact Paycor Specialist" },
                    FailedRecord = null,
                    HttpExporterResult = null,
                    RowNumber = 0,
                    ImportType = mapping.MappingName
                };
                if (_errorResultDataItems.Count == 0)
                    _errorResultDataItems.Add(errorResultData);

                return new PreparePayloadResponse
                {
                    Status = Status.Failure,
                    Error = exception,
                    ErrorResultDataItems = _errorResultDataItems
                };
            }
        }
    }
}
