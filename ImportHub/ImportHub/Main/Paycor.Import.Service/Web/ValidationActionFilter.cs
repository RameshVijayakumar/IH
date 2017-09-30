using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Paycor.Import.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Paycor.Import.Service.Web
{
    /// <summary>
    /// Forces model validation on all controllers.
    /// </summary>
    public class ValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest,
                    new ErrorResponse
                    {
                        CorrelationId = Guid.NewGuid(),
                        Title = "Validation Error",
                        Detail = "One or more validation errors occurred. See the source for more information about specific errors.",
                        Source = GenerateSourceErrorsFrom(actionContext.ModelState)
                    });
            }
        }

        private IDictionary<string, string> GenerateSourceErrorsFrom(ModelStateDictionary modelState)
        {
            var items = new Dictionary<string, string>();

            var listOfErrors = modelState.ToList();
            try
            {
                foreach (var keyValuePair in listOfErrors)
                {
                    var columnName = keyValuePair.Key.Substring(keyValuePair.Key.IndexOf(".", StringComparison.Ordinal) + 1);
                    var listofErrors = keyValuePair.Value.Errors.Aggregate(string.Empty, (current, error) => current + (error.Exception?.Message ?? error.ErrorMessage));

                    items.Add(columnName, listofErrors);
                }
            }
            catch (Exception)
            {
                // eat the exception
            }

            return items;
        }
    }
}