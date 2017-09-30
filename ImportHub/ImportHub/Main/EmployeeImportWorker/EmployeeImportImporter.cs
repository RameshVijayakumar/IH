using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Paycor.Import;
using Paycor.Import.Azure;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.Messaging;
using Paycor.Import.Status;
using Paycor.Import.Employee.Status;
using Paycor.Import.ImportHistory;
using Paycor.Import.Extensions;


namespace EmployeeImportWorker
{
    public class EmployeeImportImporter : BlobImporter<FileTranslationData<IDictionary<string, string>>, EeImportFileUploadMessage>
    {
        private string[] CRLF = new string[] { "\r\n" };
        private const string Tab = "\t";
        private readonly IStatusStorageProvider _storageProvider;
        private readonly string _container;
        private EmployeeImportStatusLogger _statusLogger;
        private EmployeeImportStatusMessage _statusMessage;
        private const int MaxFieldsLength = 46;
        private readonly IImportHistoryService _importHistoryService;

        public EmployeeImportImporter(ILog log, IStatusStorageProvider storageProvider, IImportHistoryService importHistoryService)
            : base(log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(storageProvider, nameof(storageProvider));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));

            _storageProvider = storageProvider;
            _container = ContainerNames.ImportStatus;
            _importHistoryService = importHistoryService;
            Ensure.ThatArgumentIsNotNull(_container, nameof(_container));

            Log.Debug("EmployeeImport Importer initialized.");
        }

        protected override async Task<FileTranslationData<IDictionary<string, string>>> ImportDataAsync(
                        EeImportFileUploadMessage employeeImportUploadMessage, TextReader textReader)
        {
            Ensure.ThatArgumentIsNotNull(employeeImportUploadMessage, nameof(employeeImportUploadMessage));

            // ReSharper disable once PossibleNullReferenceException - handled with Enforce
            _statusLogger = EmployeeImportStatus.GetStatusEngine(employeeImportUploadMessage.TransactionId,
                _container, _storageProvider, _importHistoryService);

            LogicalThreadContext.Properties[ImportConstants.Transaction] = employeeImportUploadMessage.TransactionId;
            LogicalThreadContext.Properties[ImportConstants.File] = employeeImportUploadMessage.UploadedFileName;
            int count = -1;

            // ** IMPORTANT **
            // The impact of an unhandled exception to the user interface must be minimized by
            // moving as much processing of this method inside the try/catch block. The Status Engine must be available
            // to the try and the catch block so it will exist outside of the block.
            try
            {
                var lineClientId = 0;

                Log.DebugFormat("Starting import for file {0}", employeeImportUploadMessage.UploadedFileName);
                _statusMessage = _statusLogger.RetrieveMessage();
                _statusMessage.Status = EmployeeImportStatusEnum.InitiateEmployeeImport;

                await _statusLogger.LogMessageAsync(_statusMessage);

                var results = new EeImportFileTranslationData<IDictionary<string, string>>()
                {
                    TransactionId = employeeImportUploadMessage.TransactionId,
                    Name = employeeImportUploadMessage.UploadedFileName,
                    ProcessingStartTime = DateTime.Now,
                    MasterSessionId = employeeImportUploadMessage.MasterSessionId,
                    MappingValue = employeeImportUploadMessage.MappingValue
                };

                var entireText = textReader.ReadToEnd();
                var entireTextArray = entireText.Split(CRLF, StringSplitOptions.RemoveEmptyEntries);
                count = entireTextArray.Length;
                var data = new List<IDictionary<string, string>>();
                var allClientIds = new List<int>();
                foreach (var line in entireTextArray)
                {
                    var lineArray = new string[MaxFieldsLength];
                    var rawLineArray = line.Split(Tab.ToCharArray(), StringSplitOptions.None);

                    if (line.ContainsDoubleQuotes())
                    {
                        for (int i = 0; i <= rawLineArray.Length - 1; i++)
                        {
                            rawLineArray[i] = rawLineArray[i].ReplaceQuotes();
                        }
                    }
                    rawLineArray.CopyTo(lineArray, 0);

                    bool isLineClientIdInt;
                    VerifyClientId(lineArray, out lineClientId, out isLineClientIdInt, allClientIds);

                    int lineEmployeeNumber;
                    bool isLineEmployeeNumberInt;
                    VerifyEmployeeNumber(lineArray, out lineEmployeeNumber, out isLineEmployeeNumberInt);

                    DateTime lineEffectiveDate;
                    bool isLineEffectiveDateDateTime;
                    VerifyEffectiveDate(lineArray, out lineEffectiveDate, out isLineEffectiveDateDateTime);

                    short lineSequenceNumber;
                    bool isLineSequenceNumberShort;
                    VerifySequenceNumber(lineArray, out lineSequenceNumber, out isLineSequenceNumberShort);

                    if (isLineClientIdInt && isLineEmployeeNumberInt && isLineEffectiveDateDateTime && isLineSequenceNumberShort)
                    {
                        var record = new Dictionary<string, string>
                        {
                            {"RecordType", lineArray[0]},
                            {"ModificationType", lineArray[1].ToUpper()},
                            {"ClientId", lineClientId.ToString(CultureInfo.InvariantCulture)},
                            {"EmployeeNumber", lineEmployeeNumber.ToString(CultureInfo.InvariantCulture)},
                            {"LastName", lineArray[4]},
                            {"EffectiveDate", lineEffectiveDate.ToString(CultureInfo.InvariantCulture)},
                            {"SeqNumber", lineSequenceNumber.ToString(CultureInfo.InvariantCulture)},
                            {"Field1", LoadNullIfEmpty(lineArray[7])},
                            {"Field2", LoadNullIfEmpty(lineArray[8])},
                            {"Field3", LoadNullIfEmpty(lineArray[9])},
                            {"Field4", LoadNullIfEmpty(lineArray[10])},
                            {"Field5", LoadNullIfEmpty(lineArray[11])},
                            {"Field6", LoadNullIfEmpty(lineArray[12])},
                            {"Field7", LoadNullIfEmpty(lineArray[13])},
                            {"Field8", LoadNullIfEmpty(lineArray[14])},
                            {"Field9", LoadNullIfEmpty(lineArray[15])},
                            {"Field10", LoadNullIfEmpty(lineArray[16])},
                            {"Field11", LoadNullIfEmpty(lineArray[17])},
                            {"Field12", LoadNullIfEmpty(lineArray[18])},
                            {"Field13", LoadNullIfEmpty(lineArray[19])},
                            {"Field14", LoadNullIfEmpty(lineArray[20])},
                            {"Field15", LoadNullIfEmpty(lineArray[21])},
                            {"Field16", LoadNullIfEmpty(lineArray[22])},
                            {"Field17", LoadNullIfEmpty(lineArray[23])},
                            {"Field18", LoadNullIfEmpty(lineArray[24])},
                            {"Field19", LoadNullIfEmpty(lineArray[25])},
                            {"Field20", LoadNullIfEmpty(lineArray[26])},
                            {"Field21", LoadNullIfEmpty(lineArray[27])},
                            {"Field22", LoadNullIfEmpty(lineArray[28])},
                            {"Field23", LoadNullIfEmpty(lineArray[29])},
                            {"Field24", LoadNullIfEmpty(lineArray[30])},
                            {"Field25", LoadNullIfEmpty(lineArray[31])},
                            {"Field26", LoadNullIfEmpty(lineArray[32])},
                            {"Field27", LoadNullIfEmpty(lineArray[33])},
                            {"Field28", LoadNullIfEmpty(lineArray[34])},
                            {"Field29", LoadNullIfEmpty(lineArray[35])},
                            {"Field30", LoadNullIfEmpty(lineArray[36])},
                            {"Field31", LoadNullIfEmpty(lineArray[37])},
                            {"Field32", LoadNullIfEmpty(lineArray[38])},
                            {"Field33", LoadNullIfEmpty(lineArray[39])},
                            {"Field34", LoadNullIfEmpty(lineArray[40])},
                            {"Field35", LoadNullIfEmpty(lineArray[41])},
                            {"Field36", LoadNullIfEmpty(lineArray[42])},
                            {"Field37", LoadNullIfEmpty(lineArray[43])},
                            {"Field38", LoadNullIfEmpty(lineArray[44])},
                            {"Field39", LoadNullIfEmpty(lineArray[45])}
                        };
                        data.Add(record);
                    }
                }
                count = data.Count;
                results.Records = data;

                _statusMessage.ClientId = lineClientId.ToString(CultureInfo.InvariantCulture);
                LogicalThreadContext.Properties[ImportConstants.Client] = _statusMessage.ClientId;

                await _statusLogger.LogMessageAsync(_statusMessage);

                VerifyDuplicateEmployeeRecordsDoesNotExist(results.Records);

                Log.Debug("File prevalidation has completed.");

                return results;
            }
            catch (Exception ex)
            {
                var importEx = ex as ImportException;
                if (importEx == null)
                {
                    _statusMessage.Message = ImportResource.FileUploadInvalidFile;
                }
                var detailItem = new EmployeeImportStatusDetail
                {
                    EmployeeName = "All",
                    EmployeeNumber = "All",
                    Issue = _statusMessage.Message,
                    RecordUploaded = false,
                    IssueType = EmployeeImportIssueTypeEnum.Error.ToString()
                };
                var details = new List<EmployeeImportStatusDetail> { detailItem };
                _statusMessage.StatusDetails = details;
                _statusMessage.PercentComplete = ImportConstants.FullCompletedPercentage;
                _statusMessage.Total = ImportConstants.TotalSteps;
                _statusMessage.FailRecordsCount = count;
                _statusMessage.SuccessRecordsCount = 0;
                _statusMessage.Status = EmployeeImportStatusEnum.ImportComplete;
                _statusLogger.LogMessage(_statusMessage);

                Log.Error(_statusMessage.Message, ex);
                throw;
            }
        }
        private static string LoadNullIfEmpty(string inputValue)
        {
            return string.IsNullOrEmpty(inputValue) ? null : inputValue;
        }

        private void VerifySequenceNumber(IReadOnlyList<string> lineArray, out short lineSequenceNumber, out bool isLineSequenceNumberShort)
        {
            isLineSequenceNumberShort = short.TryParse(lineArray[6], out lineSequenceNumber);

            if (isLineSequenceNumberShort == false)
            {
                _statusMessage.Message = EmployeeImportResource.EEImportInvalidSequence;
                throw new ImportException(_statusMessage.Message);
            }
        }

        private void VerifyEffectiveDate(IReadOnlyList<string> lineArray, out DateTime lineEffectiveDate, out bool isLineEffectiveDateDateTime)
        {
            isLineEffectiveDateDateTime = DateTime.TryParse(lineArray[5], out lineEffectiveDate);

            if (isLineEffectiveDateDateTime == false)
            {
                _statusMessage.Message = EmployeeImportResource.EEImportInvalidEffectiveDate;
                throw new ImportException(_statusMessage.Message);
            }
        }

        private void VerifyEmployeeNumber(IReadOnlyList<string> lineArray, out int lineEmployeeNumber, out bool isLineEmployeeNumberInt)
        {
            isLineEmployeeNumberInt = int.TryParse(lineArray[3], out lineEmployeeNumber);

            if (isLineEmployeeNumberInt == false)
            {
                _statusMessage.Message = EmployeeImportResource.EEImportInvalidEENumber;
                throw new ImportException(_statusMessage.Message);
            }
        }

        private void VerifyClientId(IReadOnlyList<string> lineArray, out int lineClientId, out bool isLineClientIdInt, ICollection<int> allClientIds)
        {
            isLineClientIdInt = int.TryParse(lineArray[2], out lineClientId);

            if (isLineClientIdInt == false)
            {
                _statusMessage.Message = EmployeeImportResource.EEImportInvalidClientId;
                throw new ImportException(_statusMessage.Message);
            }
            
            if (!allClientIds.Contains(lineClientId) && allClientIds.Any())
            {
                _statusMessage.Message = EmployeeImportResource.EEImportMultipleClientIds;
                throw new ImportException(_statusMessage.Message);
            }
            allClientIds.Add(lineClientId);
            
        }

        private void VerifyDuplicateEmployeeRecordsDoesNotExist(IEnumerable<IDictionary<string, string>> records)
        {
            Log.Debug("Checking for duplicates.");
            var enumerable = records as IList<IDictionary<string, string>> ?? records.ToList();
            var duplicateEmployeesRecord = (from importRecord in enumerable
                                            group importRecord by new
                                            {
                                                EmployeeNumber = importRecord["EmployeeNumber"],
                                                RecordType = importRecord["RecordType"],
                                                ClientId = importRecord["ClientId"],
                                                LastName = importRecord["LastName"],
                                                EffectiveDate = importRecord["EffectiveDate"],
                                                SeqNumber = importRecord["SeqNumber"],
                                                ModificationType = importRecord["ModificationType"]
                                            }
                                                into g
                                            where g.Count() > 1
                                            select g.Key).FirstOrDefault();

            if (duplicateEmployeesRecord != null)
            {
                _statusMessage.Message = string.Format(EmployeeImportResource.EEImportDuplicateRecords,
                    duplicateEmployeesRecord.EmployeeNumber, duplicateEmployeesRecord.RecordType);

                throw new ImportException(_statusMessage.Message);
            }
        }
    }
}
