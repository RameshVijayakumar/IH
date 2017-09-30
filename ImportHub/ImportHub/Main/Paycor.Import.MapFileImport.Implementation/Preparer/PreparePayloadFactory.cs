using System.Collections.Generic;
using log4net;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public class PreparePayloadFactory : BasePreparePayloadFactory
    {
        private readonly IRouteParameterFormatter _routeParameterFormatter;
        private readonly IApiRecordJsonGenerator _apiRecordJsonGenerator;
        private readonly IGenerateFailedRecord _generateFailedRecord;
        private readonly ICalculate _calculate;
        private readonly ILog _logger;
        public PreparePayloadFactory(IRouteParameterFormatter routeParameterFormatter, ILog logger,
            IApiRecordJsonGenerator apiRecordJsonGenerator, IGenerateFailedRecord generateFailedRecord, ICalculate calculate)
        {
            Ensure.ThatArgumentIsNotNull(routeParameterFormatter, nameof(routeParameterFormatter));
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(apiRecordJsonGenerator, nameof(apiRecordJsonGenerator));
            Ensure.ThatArgumentIsNotNull(generateFailedRecord, nameof(generateFailedRecord));
            Ensure.ThatArgumentIsNotNull(calculate, nameof(calculate));

            _apiRecordJsonGenerator = apiRecordJsonGenerator;
            _generateFailedRecord = generateFailedRecord;

            _routeParameterFormatter = routeParameterFormatter;
            _logger = logger;
            _calculate = calculate;
        }

        public override void LoadHandlers()
        {
            AddPreparePayloadExtracters(
                new List<IPayloadExtracter>
                {
                    new BatchPayloadPreparer(_apiRecordJsonGenerator, _logger, _routeParameterFormatter),
                    new NonBatchPayloadPreparer(_routeParameterFormatter, _logger, _apiRecordJsonGenerator,
                        _generateFailedRecord, _calculate)
                });
        }
    }
}