namespace Paycor.Import.ImportHubTest.Common
{
    public static class Constants
    {
        public const double Timeout = 10.0; //in minute
        public const string FileExtCsv = "csv";
        public const string FileExtXlsx = "xlsx";
        public const string TestMessageHeader = "ExpectedMessage";
        public const string ControlColumn = "ControlColumn";
        public const string MediaTypeJson = @"application/json";
        public const string MediaTypeXml = @"text/xml";
        public const string MediaTypeStream = @"application/octet-stream";
        public const string ApiAuthPath = @"/Accounts/Authentication/DoLogin?ReturnUrl=";
        public const string ApiLoginPath = @"/accounts/api/v1/me";
        public const string ApiAutoPath = @"/Accounts/Authentication/AutoLogin";
        public const string ApiKeyPath = "/accounts/api/session/getavailableclients?codeBaseId=2";
    }
}
