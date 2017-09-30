using System.Configuration;
using System.Web.Mvc;
using log4net;
using Paycor.Import.Security;
using Paycor.Security.Principal;

namespace Paycor.Import.Web.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private const string WebApiUrl = "WebApiUrl";
        private const string FileUploadApiCtrl = "FileUploadApiCtrl";
        private readonly ILog _log = LogManager.GetLogger("Import Hub Web Application");
        private PaycorUserPrincipal _principal;
        private SecurityValidator _securityValidator;

        private SecurityValidator SecurityValidator
        {
            get { return _securityValidator ?? (_securityValidator = new SecurityValidator(_log, _principal)); }
        }

        public ActionResult Index()
        {
            var result = View();
           _principal = HttpContext.User as PaycorUserPrincipal;

            if (!SecurityValidator.IsUserAuthorizedForEeImport())
            {
                _log.Error(WebResource.UnauthorizedAccess);
                return new HttpUnauthorizedResult(WebResource.UnauthorizedAccess);
            }

            var webApiUrl = ConfigurationManager.AppSettings[WebApiUrl];
            var fileUploadApiCtrl = ConfigurationManager.AppSettings[FileUploadApiCtrl];

            var finalUrl = GetConcatString(webApiUrl, fileUploadApiCtrl);

            ViewBag.WebApiFileUploadFullUrl = finalUrl;

            return result;
        }

        private static string GetConcatString(string firstString, string secondString)
        {
            string result = null;

            if ((!string.IsNullOrEmpty(firstString)) && (!string.IsNullOrEmpty(secondString)))
            {
                result = string.Concat(firstString.Trim(), secondString.Trim()).Trim();
            }
            return result;
        }

    }
}