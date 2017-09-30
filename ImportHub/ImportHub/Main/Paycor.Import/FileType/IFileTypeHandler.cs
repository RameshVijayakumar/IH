using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using Paycor.Security.Principal;
using Paycor.Import.Status;

namespace Paycor.Import.FileType
{
    public interface IFileTypeHandler
    {
        string HandleFile(FileTypeInfo fileTypeInfo, HttpPostedFile postedFile, PaycorUserPrincipal principal);

        string HandleFile(FileTypeInfo fileTypeInfo, MultipartFileData multipartFileData, PaycorUserPrincipal principal);

        FileTypeEnum ImportFileType { get; }

        FileTypeInfo GetTypeInfo(string headerLine, PaycorUserPrincipal principal,
            IEnumerable<string> sampleData = null,
            string objectType = null,
            FileTypeSortOrder sortOrder = FileTypeSortOrder.Alpha,
            AlgorithmType algorithmType = AlgorithmType.Legacy);

        ImportStatusMessage SetResultsLink(string id, PaycorUserPrincipal principal);

        ImportStatusMessage ModifyMessage(ImportStatusMessage data);

        void Cancel(string user, string transactionId, DateTime importStart);
    }
}