using System;
using System.Collections.Generic;
using System.Configuration;
using Paycor.SystemCheck;
using Paycor.Import.Mapping;
using Newtonsoft.Json;

namespace Paycor.Import.Azure
{
    public class ApiMappingDatabaseEnvironmentValidator : IEnvironmentValidator
    {
        private readonly IDocumentDbRepository<GeneratedMapping> _repository;

        public ApiMappingDatabaseEnvironmentValidator(IDocumentDbRepository<GeneratedMapping> repository)
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
                        CurrentSetting = $"Database name: {database} Collection: {collection}",
                        Name = "Mappings DocumentDb",
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
                        CurrentSetting = $"Database name: {database} Collection: {collection}",
                        Name = "Mappings DocumentDb",
                        Result = EnvironmentValidationResult.Fail,
                        AdditionalInformation = e.ToString()
                    }
                };
            }
        }
    }
}
