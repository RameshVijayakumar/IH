using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Web;
using log4net;
using Paycor.Import.Azure.Adapter;
using Paycor.Import.ImportHistory;
using Paycor.Import.Security;
using Paycor.Import.FileType;
using Paycor.Security.Principal;
using Paycor.Import.Status;


namespace Paycor.Import.Service.FileType
{
    public abstract class BaseFileTypeHandler : IFileTypeHandler
    {

        private SecurityValidator _securityValidator;

        protected readonly ILog Log;
        protected readonly IStoreFile FileStorage;
        protected readonly ICloudMessageClient<Messaging.FileUploadMessage> FileProcessor;
        protected readonly IImportHistoryService ImportHistoryService;
        protected readonly string BlobConnectionString = ConfigurationManager.AppSettings["BlobStorageConnection"];
        protected readonly string ServiceBusConnectionString;

        protected BaseFileTypeHandler(ILog log,
            IStoreFile fileStorage,
            ICloudMessageClient<Messaging.FileUploadMessage> processor,
            IImportHistoryService importHistoryService, string sbConnectionString)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(fileStorage, nameof(fileStorage));
            Ensure.ThatArgumentIsNotNull(processor, nameof(processor));
            Ensure.ThatArgumentIsNotNull(importHistoryService, nameof(importHistoryService));
            Ensure.ThatArgumentIsNotNull(sbConnectionString, nameof(sbConnectionString));
            Log = log;
            FileStorage = fileStorage;
            FileProcessor = processor;
            ImportHistoryService = importHistoryService;
            ServiceBusConnectionString = sbConnectionString;
        }

        public abstract string HandleFile(FileTypeInfo fileTypeInfo, HttpPostedFile postedFile,
            PaycorUserPrincipal principal);

        public abstract string HandleFile(FileTypeInfo fileTypeInfo, MultipartFileData multipartFileData,
            PaycorUserPrincipal principal);

        public abstract FileTypeEnum ImportFileType { get; }

        public abstract FileTypeInfo GetTypeInfo(string headerLine, PaycorUserPrincipal principal,
            IEnumerable<string> sampleData = null, string objectType = null,
            FileTypeSortOrder sortOrder = FileTypeSortOrder.Alpha,
            AlgorithmType algorithmType = AlgorithmType.Legacy);

        public abstract ImportStatusMessage SetResultsLink(string id, PaycorUserPrincipal principal);

        public abstract ImportStatusMessage ModifyMessage(ImportStatusMessage data);

        public abstract void Cancel(string user, string transactionId, DateTime importDate);

        protected static string GetFirstLine(MultipartFileData multipartFileData)
        {
            string inputLine;

            using (var reader = new StreamReader(multipartFileData.LocalFileName))
            {
                inputLine = reader.ReadLine();
            }
            return inputLine;
        }

        protected static string GetFirstLine(HttpPostedFile httpPostedFile)
        {
            var inputLine = string.Empty;

            if (httpPostedFile.ContentLength <= 0)
                return inputLine;

            using (var memoryStream = new MemoryStream())
            {
                httpPostedFile.InputStream.CopyTo(memoryStream);
                httpPostedFile.InputStream.Position = memoryStream.Position = 0;

                using (var textReader = new StreamReader(memoryStream))
                {
                    inputLine = textReader.ReadLine();
                }
            }
            return inputLine;
        }

        protected SecurityValidator GetSecurityValidator(PaycorUserPrincipal principal)
            => _securityValidator ?? (_securityValidator = new SecurityValidator(Log, principal));
    }
}