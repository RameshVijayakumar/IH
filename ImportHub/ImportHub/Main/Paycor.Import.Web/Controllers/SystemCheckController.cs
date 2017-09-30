using Paycor.Import.Web.Models;
using Paycor.SystemCheck;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Paycor.Import.Web.Controllers
{
    public class SystemCheckController : Controller
    {
        // GET: SystemCheck
        public ActionResult Pulse()
        {
            return View(new PulseCheckModel());
        }

        public ActionResult Health()
        {
            var results = new List<EnvironmentValidation>();
            results.AddRange(EnvironmentValidator.EnvironmentValidate());
            return View(new HealthCheckModel(results));
        }
    }
}