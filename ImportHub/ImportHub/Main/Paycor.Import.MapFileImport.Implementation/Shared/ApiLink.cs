using System;
using System.Linq;
using Paycor.Integration.Web;

namespace Paycor.Import.MapFileImport.Implementation.Shared
{
    public static class ApiLink
    {
        public static string GetLinkFromApi(HttpExporterResult result)
        {
            try
            {
                return
                    result?.Response?.Headers?.GetValues("Link")?
                        .SingleOrDefault(x => x.ToLower().Replace("\"", "").Replace("'", "").Contains("rel=import"));
            }
            catch (InvalidOperationException)
            {
                // Eat the possible exception that gets thrown if "Link" is not found in the response header.
                return null;
            }
        }
    }
}