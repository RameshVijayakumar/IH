using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using Paycor.Import.FileType;
using Paycor.Security.Principal;
using Paycor.Import.Status;

namespace Paycor.Import.Service.FileType
{
    public class UnrecognizedFileTypeHandler : IFileTypeHandler
    {
        public string HandleFile(FileTypeInfo fileTypeInfo, HttpPostedFile postedFile, PaycorUserPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public string HandleFile(FileTypeInfo fileTypeInfo, MultipartFileData multipartFileData,
            PaycorUserPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public FileTypeEnum ImportFileType => FileTypeEnum.Unrecognized;

        public FileTypeInfo GetTypeInfo(string headerLine, PaycorUserPrincipal principal,
            IEnumerable<string> sampleData = null, string objectType = null,
            FileTypeSortOrder sortOrder = FileTypeSortOrder.Alpha, AlgorithmType algorithmType = AlgorithmType.Legacy)
        {
            throw new NotImplementedException();
        }

        public ImportStatusMessage SetResultsLink(string id, PaycorUserPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public ImportStatusMessage ModifyMessage(ImportStatusMessage data)
        {
            throw new NotImplementedException();
        }

        public void Cancel(string user, string transactionId, DateTime importStart)
        {
            throw new NotImplementedException();
        }
    }
}