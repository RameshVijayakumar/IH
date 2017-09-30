namespace Paycor.Import
{
    public static class ImportConstants
    {
        public const string Newline = "\n";
        public const char Tab = '\t';
        public const char Pipe = '|';
        public const char Comma = ',';
        public const string Csv = "csv";
        public const string Employee = "employee";
        public const string ImportReportName = "EmployeeImportReport";
        public const string ActionFieldName = "ih:action";
        public const string ActionColumnName = "action";
        public const string UpsertAction = "U";
        public const string DefinitionPattern = "#/definitions/";
        public const string Client = "ClientId";
        public const string Transaction = "TransactionId";
        public const string File = "File";
        public const string RouteFilesGet = "files.get";
        public const string CRLF = "\\r\\n";
        public const string Space = " ";
        public const string Subquery = "Subquery";
        public const string Issue = " Issue at ";
        public const int DefaultHttpTimeout = 5;
        public const int MaxImportHistoryMessages = 1000;
        public const string PaycorRole = "Paycor";
        public const string XlsxFileExtension = ".xlsx";
        public const string CsvFileExtension = ".csv";
        public const int FullCompletedPercentage = 100;
        public const int TotalSteps = 25;
        public const string EmptyStringJsonArray = "[]";
        public const string Type = "type";
        public const string Key = "key";
        public const string Value = "value";
        public const string GlobalClientId = "0";
        public const string Period = ".";
        public const string Equal = "=";
        public const string LookUpRouteExceptionMessageKey = "LookupRoute.ExceptionMessage";
        public const string XlsxFailedRecordOriginalRowColumnName = "Original RowNumber";
        public const string XlsxFailedRecordOtherErrors = "Other Errors";
        public const string MultiType = "MultipleTypes";
        public const string ProcessCompleteMsgPurpose = "Import Hub - File Processing Complete";
        public const string ProcessCompleteMsgLong = "Your import file has been successfully processed. Please click {{here}} to view import details.";
        public const string ProcessCompleteMsgMedium = "Your import has been successfully processed.  Please login to Import Hub to view import details.";
        public const string ProcessCompleteMsgShort = "Your import has been successfully processed.  Please login to Import Hub to view import details.";
        public const string True = "true";
        public const string False = "false";
        public const string OcpApimKey = "Ocp-Apim-Subscription-Key";
        public const string EmployeeTaxImport = "Employee Tax";
        public const string EmployeeEarningImport = "Employee Earning";

        public enum EeImportMappingEnum
        {
            CancelImportOnErrorsOrWarnings,
            ContinueImport
        }
    }
    // Import Constants
}