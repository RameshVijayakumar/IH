using Paycor.Security.Principal;
using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Paycor.Import.Service.Filters
{
    public class NewRelicFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            Ensure.ThatArgumentIsNotNull(actionContext, nameof(actionContext));
                        
            if (actionContext?.Request?.RequestUri?.AbsoluteUri != null)
            {
                NewRelic.Api.Agent.NewRelic.AddCustomParameter("URL", actionContext.Request.RequestUri.AbsoluteUri);
            }


            var currentUser = CurrentUser(actionContext);

            if (currentUser != null)
            {
                NewRelic.Api.Agent.NewRelic.AddCustomParameter("UserId", currentUser.UserKey.ToString());
                NewRelic.Api.Agent.NewRelic.AddCustomParameter("UserName", currentUser.UserName);
            }

            NewRelic.Api.Agent.NewRelic.AddCustomParameter("ServerName", Environment.MachineName);
        }

        private PaycorUserPrincipal CurrentUser(HttpActionContext actionContext)
        {
            return actionContext?.RequestContext?.Principal as PaycorUserPrincipal;
        }
    }
}
