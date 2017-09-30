using Paycor.SystemCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycor.Import.Web.Models
{
    public class HealthCheckModel
    {
        private List<EnvironmentValidation> _results;

        public List<EnvironmentValidation> Environment
        {
            get { return _results; }
        }

        public HealthCheckModel(List<EnvironmentValidation> results)
        {
            _results = results;
        }
    }
}