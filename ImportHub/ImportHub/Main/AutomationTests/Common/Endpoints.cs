using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace Paycor.Import.ImportHubTest.Common
{
    public static class Endpoints
    {
        public static Uri GetEndpoint<T>(string host, string basePath, string versionPath, string query, string fragment)
        {
            string uri = $"{host}/{basePath}/{versionPath}/{query}/{fragment}";
            return new Uri(uri);
        }


        public static class IhEndPoints
        {
            #region Hardcoded endpoints
            // v1
            public static string FilePost = "/importhub/v1/files";
            public static string FileGetStatus = "/importhub/v1/files/{id}";
            public static string FileGetFailedRows = "/importhub/v1/files/{id}/failedrows";

            public static string HistoryGet = "/importhub/v1/history";
            public static string HistoryDelete = HistoryGet;

            public static string MapGet = "/importhub/v1/mappings";
            public static string MapPost = MapGet;
            public static string MapGetAll = "/importhub/v1/mappings/{id}";
            public static string MapDelete = MapGet;
            public static string MapUpdate = MapGet;

            public static string FiletypePost = "/importhub/v1/filetypes";

            public static string AnalyticsGet = "/importhub/v1/analytics";

            // unversioned
            public static string FiletypePostUnversioned = "/importhub/filetypes";
            public static string RegisteredMapsGet = "/importhub/registeredmaps";
            public static string RegisteredMapsPost = RegisteredMapsGet;

            // System check
            public static string HealthCheck = "api/SystemCheck/HealthCheck";
            public static string LogCheck = "/api/SystemCheck/LogCheck";
            public static string PiiAppenderCheck = "/api/SystemCheck/PiiAppenderCheck";
            public static string PalseCheck = "/api/SystemCheck/PulseCheck";
            #endregion

            public static string GetSwaggerDocByVersion(string host, string basePath, string version)
            {
                throw new NotImplementedException();
            }

            public static List<string> GetApiList(string swaggerDoc)
            {
                throw new NotImplementedException();
            }

            public static List<string> GetVerbList(string apiDoc)
            {
                throw new NotImplementedException();
            }
        }
    }
}
