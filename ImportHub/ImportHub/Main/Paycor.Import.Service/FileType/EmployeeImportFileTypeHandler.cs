using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using log4net;
using Newtonsoft.Json;
using Paycor.Import.Azure;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.Employee.Status;
using Paycor.Import.ImportHistory;
using Paycor.Import.JsonFormat;
using Paycor.Import.Messaging;
using Paycor.Import.Service.Shared;
using Paycor.Import.Status.Azure;
using Paycor.Security.Principal;
using Paycor.Import.FileType;
using Paycor.Import.Status;



namespace Paycor.Import.Service.FileType
{
    public class EmployeeImportFileTypeHandler : BaseFileTypeHandler
    {
        private readonly IFormatJson _jsonFormatter;

        public EmployeeImportFileTypeHandler(ILog log,
            IStoreFile fileStorage,
            ICloudMessageClient<Messaging.FileUploadMessage> processor,
            IImportHistoryService importHistoryService,
            string sbConnectionString,
            IFormatJson jsonFormatter) :
            base(log, fileStorage, processor, importHistoryService, sbConnectionString)
        {
            Ensure.ThatArgumentIsNotNull(jsonFormatter, nameof(jsonFormatter));
            _jsonFormatter = jsonFormatter;
        }

        public override FileTypeInfo GetTypeInfo(string headerLine, PaycorUserPrincipal principal,
            IEnumerable<string> sampleData = null,
            string objectType = null,
            FileTypeSortOrder sortOrder = FileTypeSortOrder.Alpha,
            AlgorithmType algorithmType = AlgorithmType.Legacy)
        {
            return new EeFileTypeInfo()
            {
                FileType = ImportFileType
            };
        }

        public override string HandleFile(FileTypeInfo fileTypeInfo, HttpPostedFile postedFile,
            PaycorUserPrincipal principal)
        {
            var headerline = GetFirstLine(postedFile);
            var clientId = ValidateUser(headerline, principal);
            var eeFileTypeInfo = (EeFileTypeInfo) fileTypeInfo;

            var clientFileName = Path.GetFileName(postedFile.FileName);
            FileStorage.StoreFile(postedFile);

            return Handle(eeFileTypeInfo.MappingValue, clientFileName, clientFileName, clientId, principal);
        }

        public override string HandleFile(FileTypeInfo fileTypeInfo, MultipartFileData multipartFileData,
            PaycorUserPrincipal principal)
        {
            var headerline = GetFirstLine(multipartFileData);
            var clientId = ValidateUser(headerline, principal);
            LogicalThreadContext.Properties[ImportConstants.Client] = clientId;
            var eeFileTypeInfo = (EeFileTypeInfo) fileTypeInfo;
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

            return Handle(eeFileTypeInfo.MappingValue, storedFileName, clientFileName, clientId, principal);
        }

        public override ImportStatusMessage SetResultsLink(string id, PaycorUserPrincipal principal)
        {
            EmployeeImportStatusMessage resultMessage;

            var storageProvider = new BlobStatusStorageProvider(BlobConnectionString, ContainerNames.ImportStatus);
            var statusLogger = EmployeeImportStatus.GetStatusEngine(id, ContainerNames.ImportStatus, storageProvider,
                ImportHistoryService);
            var message = statusLogger.RetrieveMessage();
            if (message == null)
            {
                Log.DebugFormat(ExceptionMessages.StatusMessageNotFound, id);
                resultMessage = null;
            }
            else
            {
                var clientid = message.ClientId ?? "0";

                if (message.Status == EmployeeImportStatusEnum.ImportComplete)
                {
                    message.ResultsDownloadLink = new ReportUrlGenerator().GetReportUrl(clientid,
                        ImportConstants.ImportReportName);
                }

                ValidateUser(message, principal);
                resultMessage = message;
            }
            return resultMessage;
        }

        public override FileTypeEnum ImportFileType => FileTypeEnum.EmployeeImport;

        private int ValidateUser(string headerLine, PaycorUserPrincipal principal)
        {
            var clientId = GetClientId(Log, headerLine);
            CheckUserForAutorization(clientId, principal);
            return clientId;
        }

        private void ValidateUser(EmployeeImportStatusMessage message, PaycorUserPrincipal principal)
        {
            var clientId = GetClientId(message);
            CheckUserForAutorization(clientId, principal);
        }

        private void CheckUserForAutorization(int clientId, PaycorUserPrincipal principal)
        {
            var securityValidator = GetSecurityValidator(principal);
            if (!securityValidator.IsUserAuthorizedForEeImport(clientId))
            {
                throw new UnauthorizedAccessException(ExceptionMessages.UnauthorizedAccess);
            }
        }

        private int GetClientId(EmployeeImportStatusMessage message)
        {
            int intClientId;
            var clientid = message.ClientId ?? "0";
            int.TryParse(clientid, out intClientId);

            return intClientId;
        }

        private string Handle(string mappingValue, string storedFileName, string clientFileName, int clientId,
            PaycorUserPrincipal principal)
        {
            var statusMessageId = Guid.NewGuid().ToString();
            var user = GetSecurityValidator(principal).GetUser();
            var userName = GetSecurityValidator(principal).GetUserName();
            LogicalThreadContext.Properties[ImportConstants.Transaction] = statusMessageId;
            Log.InfoFormat(ExceptionMessages.FileUploadSuccessMessage, clientFileName, ContainerNames.EmployeeImport,
                QueueNames.EmployeeImport);

            var storageProvider = new BlobStatusStorageProvider(BlobConnectionString, ContainerNames.ImportStatus);
            var statusLogger = EmployeeImportStatus.GetStatusEngine(statusMessageId, ContainerNames.ImportStatus,
                storageProvider, ImportHistoryService);

            var message = new EmployeeImportStatusMessage
            {
                Id = statusMessageId,
                FileName = clientFileName,
                Status = EmployeeImportStatusEnum.Queued,
                ClientId = clientId.ToString(),
                ImportType = ImportConstants.Employee,
                FileType = ImportFileType.ToString(),
                User = user,
                UserName = userName
            };

            statusLogger.LogMessage(message);

            FileProcessor.SendMessage(new EeImportFileUploadMessage()
                {
                    Container = ContainerNames.EmployeeImport,
                    File = storedFileName,
                    TransactionId = statusMessageId,
                    UploadedFileName = clientFileName,
                    MasterSessionId = principal.MasterSessionID.ToString(),
                    MappingValue = mappingValue
                },
                QueueNames.EmployeeImport,
                ServiceBusConnectionString);
            return statusMessageId;
        }

        private static int GetClientId(ILog log, string line)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatStringIsNotNullOrEmpty(line, nameof(line));

            int clientId;
            string errMessage;
            var lineArray = line.Split(ImportConstants.Tab);

            if (lineArray.GetUpperBound(0) < 1)
            {
                errMessage = ExceptionMessages.FileUploadNotTabDelimited;
                throw new ImportException(errMessage);
            }

            if (lineArray.GetUpperBound(0) < 2)
            {
                errMessage = ExceptionMessages.FileUploadMissingClientId;
                throw new ImportException(errMessage);
            }

            if (IsFirstLineEmpty(lineArray))
            {
                errMessage = ExceptionMessages.EEImportEmptyFirstLine;
                throw new ImportException(errMessage);
            }

            int.TryParse(lineArray[2], out clientId);
            if (clientId == 0)
            {
                errMessage = ExceptionMessages.InvalidClientId;
                throw new ImportException(errMessage);
            }

            errMessage = string.Format(ExceptionMessages.FileUploadParsedClientId, clientId);
            log.Debug(errMessage);
            return clientId;
        }

        public override ImportStatusMessage ModifyMessage(ImportStatusMessage data)
        {
            var employeeImportStatusMessage = data as EmployeeImportStatusMessage;
            if (employeeImportStatusMessage == null)
                throw new ArgumentNullException(nameof(employeeImportStatusMessage));
            var modifiedMessage =
                _jsonFormatter.RemoveProperties(JsonConvert.SerializeObject(employeeImportStatusMessage),
                    new List<string> { "StatusDetails" });

            return JsonConvert.DeserializeObject<EmployeeImportStatusMessage>(modifiedMessage);
        }

        public override void Cancel(string user, string transactionId, DateTime importDate)
        {
            throw new NotImplementedException();
        }

        private static bool IsFirstLineEmpty(IEnumerable<string> lineArray)
        {
            var isFirstLineEmpty = lineArray.All(string.IsNullOrEmpty);
            return isFirstLineEmpty;
        }
    }
}