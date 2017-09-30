using Microsoft.Owin;
using Owin;
using Paycor.Security.Owin;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

[assembly: OwinStartup(typeof(Paycor.Import.Web.Startup))]
namespace Paycor.Import.Web
{
    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            var settings = GetAuthSettingsFromWebConfig();

            app.UsePaycorAuthForWebApi(new PaycorAuthOptions
            {
                AuthenticationMode = settings.Mode.ToString(),
                LoginUrl = System.Web.Security.FormsAuthentication.LoginUrl
            });

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        private static AuthenticationSection GetAuthSettingsFromWebConfig()
        {
            var settings = (AuthenticationSection)WebConfigurationManager.GetWebApplicationSection("system.web/authentication");
            return settings;
        }
    }
}