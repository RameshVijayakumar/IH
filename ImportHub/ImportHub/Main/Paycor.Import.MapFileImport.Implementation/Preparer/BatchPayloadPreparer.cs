using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public class BatchPayloadPreparer : PayloadPreparer, IPayloadExtracter
    {
        private readonly IApiRecordJsonGenerator _apiRecordJsonGenerator;
        public PreparePayloadTypeEnum GetPreparePayloadType()
        {
            return PreparePayloadTypeEnum.Batch;
        }

        public BatchPayloadPreparer(IApiRecordJsonGenerator apiRecordJsonGenerator, ILog logger, IRouteParameterFormatter routeParameterFormatter) :base(routeParameterFormatter, logger)
        {
            Ensure.ThatArgumentIsNotNull(apiRecordJsonGenerator, nameof(apiRecordJsonGenerator));
            _apiRecordJsonGenerator = apiRecordJsonGenerator;
            
        }

        public List<PayloadData> PreparePayload(ImportContext context, IEnumerable<ApiRecord> apiRecords)
        {
            var payloadDataItems = new List<PayloadData>();
            ErrorResultDataItems = new List<ErrorResultData>();
            var enumerableApiRecords = apiRecords as ApiRecord[] ?? apiRecords.ToArray();
            var jsonData = _apiRecordJsonGenerator.SerializeRecordsListJson(enumerableApiRecords.AllRecords());
            var payloadData = new PayloadData
            {
                EndPoint = HtmlVerb.Post.GetEndPointWithVerb(context.Endpoints).Value,
                HtmlVerb = HtmlVerb.Post,
                PayLoad = jsonData,
                ApiRecords = enumerableApiRecords
            };
            payloadDataItems.Add(payloadData);
            return payloadDataItems;
        }
    }
}
