using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Paycor.SystemCheck;
using Paycor.Import.ImportHistory;

namespace Paycor.Import.Azure
{
    public class HistoryDatabaseEnvironmentValidator : IEnvironmentValidator
    {
        private readonly IDocumentDbRepository<ImportHistoryMessage> _repository;

        public HistoryDatabaseEnvironmentValidator(IDocumentDbRepository<ImportHistoryMessage> repository)
        {
            _repository = repository;
        }

        public IEnumerable<EnvironmentValidation> EnvironmentValidate()
        {
            var database = ConfigurationManager.AppSettings["database"];
            var collection = ConfigurationManager.AppSettings["collection"];
            try
            {
                return new List<EnvironmentValidation>()
                {
                    new EnvironmentValidation()
                    {
                        Name = "History DocumentDb",
                        CurrentSetting = $"Database name: {database} Collection: {collection}",
                        Result = EnvironmentValidationResult.Pass,
                    }
                };
            }
            catch(Exception e)
            {
                return new List<EnvironmentValidation>()
                {
                    new EnvironmentValidation()
                    {
                        Name = "History DocumentDb",
                        CurrentSetting = $"Database name: {database} Collection: {collection}",
                        Result = EnvironmentValidationResult.Fail,
                        AdditionalInformation = e.ToString()
                    }
                };
            }
        }

    }
}
