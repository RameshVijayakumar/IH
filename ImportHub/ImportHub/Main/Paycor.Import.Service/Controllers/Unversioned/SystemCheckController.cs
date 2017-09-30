using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using log4net;
using Paycor.Import.Attributes;
using Paycor.SystemCheck;
using Paycor.Import.Security;
using Paycor.Security.Principal;
using System.Web;
using System.Net.Http;
using System.Net;
using Paycor.Import.Azure;

namespace Paycor.Import.Service.Controllers.Unversioned
{
    public class SystemCheckController : ApiController
    {
        private readonly ILog _log;
        private readonly SecurityValidator _securityValidator;
        private readonly IEnumerable<IEnvironmentValidator> _validators;

        public SystemCheckController(ILog log, IEnumerable<IEnvironmentValidator> validators)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
            _validators = validators;
            _securityValidator = new SecurityValidator(_log, HttpContext.Current.User as PaycorUserPrincipal);
        }

        [HttpGet]
        [Route("api/SystemCheck")]
        [Route("api/SystemCheck/EnvironmentValidate")]
        public IHttpActionResult Get()
        {
            bool isAuthorized = _securityValidator.IsAuthorizedForSystemCheck();
            if(isAuthorized)
            {
                return Ok(FilteredEnvironmentValidate());
            }
            else
            {
                return Unauthorized();
            }
            
        }

        [HttpGet]
        [Route("api/SystemCheck/IsHealthy")]
        public bool IsHealthy()
        {
            var result = false;
            try
            {
                result = FilteredEnvironmentValidate().All(ev => ev.Result != EnvironmentValidationResult.Fail);
            }
            catch (Exception ex)
            {
                _log.Error($"An exception occurred while doing a {nameof(IsHealthy)} system check.", ex);
            }
            return result;
        }

        [HttpGet]
        [Route("api/SystemCheck/IsAvailable")]
        public HttpResponseMessage IsAvailable()
        {
            bool hasNoFailures = false;
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = "SystemCheck:IsAvailable - No" };
            try
            {
                hasNoFailures = FastEnvironmentValidate().All(ev => ev.Result != EnvironmentValidationResult.Fail);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                result = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = "SystemCheck:IsAvailable - Exception Thrown" };
            }

            if (hasNoFailures)
            {
                result = new HttpResponseMessage(HttpStatusCode.OK);
            }

            return result; // 200 or 503 w/ ReasonPhrase
        }

        [HttpGet]
        [Route("api/SystemCheck/HealthCheck")]
        [RolesAuthorize(ImportConstants.PaycorRole)]
        public IEnumerable<EnvironmentValidation> HealthCheck()
        {
            var results = new List<EnvironmentValidation>();
            results.AddRange(_validators.SelectMany(s => s.EnvironmentValidate()));
            _log.Info("HealthCheck called");
            return results;
        }

        [HttpGet]
        [RolesAuthorize(ImportConstants.PaycorRole)]
        [Route("api/SystemCheck/LogCheck")]
        public void LogCheck()
        {
            Trace.WriteLine("Testing Logging");
            Trace.TraceError("Testing Error Logging");
            Trace.TraceWarning("Testing Warning Logging");
            Trace.TraceInformation("Testing Information Logging");
        }

        [HttpGet]
        [RolesAuthorize(ImportConstants.PaycorRole)]
        [Route("api/SystemCheck/PiiAppenderCheck")]
        public void PiiAppenderCheck()
        {
            _log.Info("Verifying stackify appender PII logging started.");
            _log.Info("The following entries should be clean (No PII detected):");
            _log.Debug("This is a test debug statement.");
            _log.Info("This is a test info statement.");
            _log.Warn("This is a test warn statement.");
            _log.Error("This is a test error statement.");
            _log.Fatal("This is a test fatal statement.");
            try
            {
                throw new Exception("This is a test exception.");
            }
            catch (Exception ex)
            {
                _log.Error("This is a test error statement.", ex);
            }

            _log.Info("The following entries should be dirty (PII detected and masked):");
            _log.Debug("This is a test debug statement with PII 123-45-6789.");
            _log.Info("This is a test info statement 06/21/1983.");
            _log.Warn("This is a test warn statement LastName Pimm.");
            _log.Error("This is a test error statement HomePhone 214-724-1701.");
            _log.Fatal("This is a test fatal statement 06-21-95.");
            _log.Debug("{\"annualHours\":\"1880\",\"clientId\":\"102\",\"employeeNumber\":\"49983\",\"id\":\"126\",\"section125IneligibleEmployee\":\"34\",\"manager\":{ \"id\":\"126\"},\"person\":{ \"birthDate\":\"09/11/2001\",\"clientId\":\"102\",\"firstName\":\"Trixie\",\"id\":\"126\",\"lastName\":\"Sparky\",\"ssn\":\"930-03-9393\"}");
            try
            {
                throw new Exception("This is a test exception 123-45-6789.");
            }
            catch (Exception ex)
            {
                _log.Error("This is a test error statement.", ex);
            }

            try
            {
                throw new Exception("{\"annualHours\":\"1880\",\"clientId\":\"102\",\"employeeNumber\":\"49983\",\"id\":\"126\",\"section125IneligibleEmployee\":\"34\",\"manager\":{ \"id\":\"126\"},\"person\":{ \"birthDate\":\"09/11/2001\",\"clientId\":\"102\",\"firstName\":\"Trixie\",\"id\":\"126\",\"lastName\":\"Sparky\",\"ssn\":\"930-03-9393\"}");
            }
            catch (Exception ex)
            {
                _log.Error("This is a test error satement with 123-45-6789", ex);
            }

            _log.Info("Verifying stackify appender PII logging completed.");
        }

        [HttpGet]
        [RolesAuthorize(ImportConstants.PaycorRole)]
        [Route("api/SystemCheck/ThrowError")]
        public void ThrowError()
        {
            throw new Exception("Testing an unhandled exception");
        }

        [HttpGet]
        [Route("api/SystemCheck/PulseCheck")]
        public string PulseCheck()
        {
            var result = "Exception";
            try
            {
                result = PulseCheckHelper.PulseCheck();
            }
            catch (Exception ex)
            {
                _log.Error($"An exception occurred while processing the {nameof(PulseCheck)} system check.", ex);
            }
            return result;
        }

        private IEnumerable<EnvironmentValidation> FilteredEnvironmentValidate()
        {
            var results = new List<EnvironmentValidation>();
            //Run any additional checks (extneral urls, Azure assets)
            results.AddRange(_validators.SelectMany(s => s.EnvironmentValidate()));
            return results;
        }

        private IEnumerable<EnvironmentValidation> FastEnvironmentValidate()
        {            
            var results = new List<EnvironmentValidation>();
            //Run any additional checks (extneral urls, Azure assets)
            var fastValidators = _validators.Where(v => v.GetType() != typeof(ApiMappingDatabaseEnvironmentValidator) && v.GetType() != typeof(HistoryDatabaseEnvironmentValidator));
            results.AddRange(fastValidators.SelectMany(s => s.EnvironmentValidate()));
            return results;
        }

    }
}
