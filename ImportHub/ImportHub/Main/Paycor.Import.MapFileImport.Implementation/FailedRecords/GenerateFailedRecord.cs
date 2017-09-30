using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.FailedRecordFormatter;
using Paycor.Import.Http;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.FailedRecords
{
    public class GenerateFailedRecord : IGenerateFailedRecord
    {
        private static IErrorFormatter _errorFormatter;
        public GenerateFailedRecord(IErrorFormatter errorFormatter)
        {
            Ensure.ThatArgumentIsNotNull(errorFormatter, nameof(errorFormatter));
            _errorFormatter = errorFormatter;
        }

        public FailedRecord GetFailedRecord(ApiRecord apiRecord, ErrorResponse errorResponseData,
        HttpExporterResult result)
        {
            AddStringArraysFields(apiRecord, apiRecord.Record);
            AddPayLoadArrayFields(apiRecord, apiRecord.Record);
            return AddFailedRecord(apiRecord, apiRecord.RowNumber, errorResponseData, result);
        }

        private static FailedRecord AddFailedRecord(ApiRecord apiRecord, int rowNumber, ErrorResponse errorResponseData,
            HttpExporterResult result)
        {
            FailedRecord resultFailedRecord = null;
            if (errorResponseData.Source == null && result == null)
            {
                var failedRecord = new FailedRecord
                {
                    Record = apiRecord.Record,
                    CustomData = new Dictionary<string, string>((StringComparer.OrdinalIgnoreCase))
                    {
                        {
                            ImportConstants.XlsxFailedRecordOriginalRowColumnName, rowNumber.ToString()
                        },
                        {
                            ImportConstants.XlsxFailedRecordOtherErrors, errorResponseData.Detail
                        }
                    }
                };
                resultFailedRecord = failedRecord;
                return resultFailedRecord;
            }
            if ((errorResponseData.Source == null) && result.Response != null)
            {
                var failedRecord = new FailedRecord
                {
                    Record = apiRecord.Record,
                    CustomData = new Dictionary<string, string>((StringComparer.OrdinalIgnoreCase))
                    {
                        {
                            ImportConstants.XlsxFailedRecordOriginalRowColumnName, rowNumber.ToString()
                        },
                        {
                            ImportConstants.XlsxFailedRecordOtherErrors,
                            _errorFormatter.FormatError(errorResponseData, result)                
                        }
                    }
                };

                resultFailedRecord = failedRecord;
                return resultFailedRecord;
            }

            if (errorResponseData.Source == null || !errorResponseData.Source.Any())
            {
                var failedRecord = new FailedRecord
                {
                    Record = apiRecord.Record,
                    CustomData = new Dictionary<string, string>((StringComparer.OrdinalIgnoreCase))
                    {
                        {
                            ImportConstants.XlsxFailedRecordOriginalRowColumnName, rowNumber.ToString()
                        },
                        {
                             ImportConstants.XlsxFailedRecordOtherErrors, errorResponseData.Detail
                        }
                    }
                };
                resultFailedRecord = failedRecord;
            }
            else if (errorResponseData.Source != null)
            {
                var failedRecord = new FailedRecord
                {
                    Record = apiRecord.Record,
                    Errors = errorResponseData.Source,
                    CustomData = new Dictionary<string, string>((StringComparer.OrdinalIgnoreCase))
                    {
                        {
                            ImportConstants.XlsxFailedRecordOriginalRowColumnName, rowNumber.ToString()
                        }
                    }
                };
                resultFailedRecord = failedRecord;
            }
            return resultFailedRecord;
        }

        private static void AddPayLoadArrayFields(ApiRecord apiRecord, IDictionary<string, string> record)
        {
            if (apiRecord?.ApiPayloadArrays == null) return;
            foreach (var apiPayloadArray in apiRecord.ApiPayloadArrays)
            {
                var count = 0;
                if (apiPayloadArray.ArrayData == null) continue;
                foreach (var arrayData in apiPayloadArray.ArrayData.ToList())
                {
                    if (arrayData == null) continue;
                    count++;
                    foreach (var array in arrayData)
                    {
                        record.Add(new KeyValuePair<string, string>(apiPayloadArray.ArrayName + array.Key + count, array.Value));
                    }
                }
            }
        }

        private static void AddStringArraysFields(ApiRecord apiRecord, IDictionary<string, string> record)
        {
            if (apiRecord?.ApiPayloadStringArrays == null) return;
            foreach (var apiPayloadStringArray in apiRecord.ApiPayloadStringArrays)
            {
                var count = 0;
                if (apiPayloadStringArray.StringArrayData == null) continue;
                foreach (var stringArray in apiPayloadStringArray.StringArrayData)
                {
                    record.Add(new KeyValuePair<string, string>(apiPayloadStringArray.StringArrayName + count++, stringArray));
                }
            }
        }
    }
}
