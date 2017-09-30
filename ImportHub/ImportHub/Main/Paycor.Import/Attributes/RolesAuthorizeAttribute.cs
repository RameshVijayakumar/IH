using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
// TODO: No unit tests
namespace Paycor.Import.Attributes
{
    public class RolesAuthorizeAttribute : AuthorizeAttribute
    {
        public string CommaSepartedRoles { get; set; }
        public RolesAuthorizeAttribute(string commaSeparatedRoles)
        {
            CommaSepartedRoles = commaSeparatedRoles;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!base.IsAuthorized(actionContext))
                return false;

            var identity = Thread.CurrentPrincipal.Identity;
            if (identity == null && HttpContext.Current != null)
            {
                identity = HttpContext.Current.User.Identity;
            }

            if (identity == null || !identity.IsAuthenticated)
                return false;

            if (CommaSepartedRoles == null)
                return false;

            var roles = CommaSepartedRoles.Split(',');
            return roles.Any(role => HttpContext.Current.User.IsInRole(role));
        }
    }   
}
