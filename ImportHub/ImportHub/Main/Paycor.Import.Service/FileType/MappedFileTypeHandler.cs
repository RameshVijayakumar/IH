using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Azure;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.Extensions;
using Paycor.Import.FileType;
using Paycor.Import.ImportHistory;
using Paycor.Import.JsonFormat;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using Paycor.Import.Service.Shared;
using Paycor.Import.Status;
using Paycor.Security.Principal;

namespace Paycor.Import.Service.FileType
{
    public class MappedFileTypeHandler : BaseFileTypeHandler
    {
        private readonly IFieldMapper _fieldMapper;
        private readonly IImportHistoryService _importHistoryService;
        private readonly IFormatJson _jsonFormatter;
        private readonly IStoreData<ImportCancelToken> _cancelTokenStorage;
        private readonly IStatusStorageProvider _statusStorage;

        public MappedFileTypeHandler(ILog log,
            IStoreFile fileStorage,
            ICloudMessageClient<Messaging.FileUploadMessage> fileProcessor,
            IImportHistoryService importHistoryService,
            IFieldMapper fieldMapper,
            string sbConnectionString,
            IFormatJson jsonFormatter,
            IStoreData<ImportCancelToken> cancelTokenStorage,
            IStatusStorageProvider statusStorage) :
            base(log, fileStorage, fileProcessor, importHistoryService, sbConnectionString)
        {
            Ensure.ThatArgumentIsNotNull(fieldMapper, nameof(fieldMapper));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            Ensure.ThatArgumentIsNotNull(jsonFormatter, nameof(jsonFormatter));
            Ensure.ThatArgumentIsNotNull(cancelTokenStorage, nameof(cancelTokenStorage));
            Ensure.ThatArgumentIsNotNull(statusStorage, nameof(statusStorage));

            _fieldMapper = fieldMapper;
            _importHistoryService = importHistoryService;
            _jsonFormatter = jsonFormatter;
            _cancelTokenStorage = cancelTokenStorage;
            _statusStorage = statusStorage;
        }

        public override string HandleFile(FileTypeInfo mappedFileTypeInfo, HttpPostedFile postedFile,
            PaycorUserPrincipal principal)
        {
            var clientFileName = Path.GetFileName(postedFile.FileName);
            FileStorage.StoreFile(postedFile);
            return Handle(clientFileName, clientFileName, mappedFileTypeInfo, principal);
        }

        public override string HandleFile(FileTypeInfo mappedFileTypeInfo, MultipartFileData multipartFileData,
            PaycorUserPrincipal principal)
        {
            var storedFileName = Path.GetFileName(multipartFileData.LocalFileName);
            string clientFileName;
            try
            {
                clientFileName = multipartFileData.Headers.ContentDisposition.FileName.Trim('\"');
                clientFileName = Path.GetFileName(clientFileName);

            }
            catch (Exception)
            {
                clientFileName = storedFileName;
            }

            FileStorage.StoreFile(multipartFileData);
            LogicalThreadContext.Properties[ImportConstants.File] = clientFileName;
            return Handle(storedFileName, clientFileName, mappedFileTypeInfo, principal);
        }

        public override FileTypeEnum ImportFileType => FileTypeEnum.MappedFileImport;

        public override FileTypeInfo GetTypeInfo(string headerLine, PaycorUserPrincipal principal,
            IEnumerable<string> sampleData = null, string objectType = null,
            FileTypeSortOrder sortOrder = FileTypeSortOrder.Alpha,
            AlgorithmType algorithmType = AlgorithmType.Legacy)
        {
            var columnCount = 0;
            var isCustomMap = false;

            var fields = headerLine.Split(ImportConstants.Comma);
            for (var i = 0; i < fields.Length; i++)
            {
                // Allow field names with spaces by removing the spaces to determine mapping match
                fields[i] = fields[i].RemoveWhiteSpaces();
            }

            IDictionary<string, int?> mapRankings;
            IEnumerable<ApiMapping> mappings =
                _fieldMapper.GetMappingDefinitions(fields, principal, out mapRankings, objectType, algorithmType,
                    sortOrder).ToArray();
            if (!mappings.Any())
            {
                isCustomMap = true;
                mappings = _fieldMapper.GetAllApiMappings(principal, objectType);
            }

            IEnumerable<ApiMapping> allMappings = _fieldMapper.GetAllApiMappings(principal, objectType).ToArray();

            if (sampleData != null)
            {
                columnCount = FileTypeRecognizer.GetMaxNumberOfColumns(sampleData, ImportConstants.Comma);
            }

            return new MappedFileTypeInfo
            {
                FileType = ImportFileType,
                IsCustomMap = isCustomMap,
                Mappings = mappings,
                AllMappings = allMappings,
                ColumnCount = columnCount,
                MapRankings = mapRankings
            };
        }

        public override void Cancel(string user, string transactionId, DateTime importDate)
        {
            Ensure.ThatStringIsNotNullOrEmpty(user, nameof(user));
            Ensure.ThatStringIsNotNullOrEmpty(transactionId, nameof(transactionId));

            _cancelTokenStorage.Store(new ImportCancelToken
            {
                CancelRequested = true,
                TransactionId = transactionId
            });

            var statusLogger = MappedFileImportStatus.GetStatusEngine(transactionId, ContainerNames.ImportStatus,
                _statusStorage,
                ImportHistoryService);

            var blobStatus = statusLogger.RetrieveMessage();

            //this is needed for UI to show user cancel in progress
            blobStatus.IsImportCancelled = true;

            if (DateTime.Now.Subtract(importDate).Days >= 1)
            {
                blobStatus.Status = MappedFileImportStatusEnum.ImportComplete;
                blobStatus.PercentComplete = 100;
            }
            statusLogger.LogMessage(blobStatus);

        }

        private string Handle(string storedFileName, string clientFileName, IFileTypeInfo info,
            PaycorUserPrincipal principal)
        {
            var statusMessageId = Guid.NewGuid().ToString();
            var user = GetSecurityValidator(principal).GetUser();
            var userName = GetSecurityValidator(principal).GetUserName();
            LogicalThreadContext.Properties[ImportConstants.Transaction] = statusMessageId;
            var mappedFileTypeInfo = (MappedFileTypeInfo) info;

            if (!mappedFileTypeInfo.Mappings.Any())
            {
                throw new ImportException(ExceptionMessages.NoMappingFound);
            }

            Log.InfoFormat(ExceptionMessages.FileUploadSuccessMessage, storedFileName, ContainerNames.MappedFileImport,
                QueueNames.MappedFileImport);

            var statusLogger = MappedFileImportStatus.GetStatusEngine(statusMessageId, ContainerNames.ImportStatus,
                _statusStorage, _importHistoryService);

            var mappings = mappedFileTypeInfo.Mappings.ToList();

            if (mappedFileTypeInfo.IsMultiSheet)
            {
                var message = new MappedFileImportStatusMessage
                {
                    Id = statusMessageId,
                    FileName = clientFileName,
                    Status = MappedFileImportStatusEnum.Queued,
                    PercentComplete = 0.0M,
                    ImportType = ImportConstants.MultiType,
                    FileType = ImportFileType.ToString(),
                    User = user,
                    UserName = userName,
                    Source = mappedFileTypeInfo.Source
                };

                statusLogger.LogMessage(message);

                FileProcessor.SendMessage(new MultiSheetImportStatusMessage
                    {
                        Container = ContainerNames.MappedFileImport,
                        File = storedFileName,
                        UploadedFileName = clientFileName,
                        TransactionId = statusMessageId,
                        MasterSessionId = principal.MasterSessionID.ToString(),
                        BaseMappings = mappings
                },
                    QueueNames.MultiFileImport, ServiceBusConnectionString);
            }
            else
            {
                var mapping = mappings.First();
                var message = new MappedFileImportStatusMessage
                {
                    Id = statusMessageId,
                    FileName = clientFileName,
                    Status = MappedFileImportStatusEnum.Queued,
                    PercentComplete = 0.0M,
                    ImportType = mapping.MappingName,
                    FileType = ImportFileType.ToString(),
                    User = user,
                    UserName = userName,
                    Source = mappedFileTypeInfo.Source
                };

                statusLogger.LogMessage(message);

                FileProcessor.SendMessage(new MappedImportFileUploadMessage
                    {
                        Container = ContainerNames.MappedFileImport,
                        File = storedFileName,
                        UploadedFileName = clientFileName,
                        TransactionId = statusMessageId,
                        MasterSessionId = principal.MasterSessionID.ToString(),
                        ApiMapping = mapping
                },
                    QueueNames.MappedFileImport, ServiceBusConnectionString);
            }
            return statusMessageId;
        }

        public override ImportStatusMessage SetResultsLink(string id, PaycorUserPrincipal principal)
        {
            var statusLogger = MappedFileImportStatus.GetStatusEngine(id, ContainerNames.ImportStatus, _statusStorage,
                ImportHistoryService);

            var message = statusLogger.RetrieveMessage();
            if (message != null)
                return message;

            Log.DebugFormat(ExceptionMessages.StatusMessageNotFound, id);
            return null;
        }

        public override ImportStatusMessage ModifyMessage(ImportStatusMessage data)
        {
            var mappedFileStatusMessage = data as MappedFileImportStatusMessage;
            if (mappedFileStatusMessage == null) throw new ArgumentNullException(nameof(mappedFileStatusMessage));
            var modifiedMessage = _jsonFormatter.RemoveProperties(JsonConvert.SerializeObject(mappedFileStatusMessage),
                new List<string> {"StatusDetails"});

            return JsonConvert.DeserializeObject<MappedFileImportStatusMessage>(modifiedMessage);
        }
    }
}