using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Mapping;

//TODO: No unit tests

namespace Paycor.Import.Validator
{
    public class MappedFileMappingValidator : IValidator<ApiMapping>
    {
        private readonly ILog _logger;

        public IEnumerable<IValidator<MappingDefinition>> MappingValidators { get; }

        public MappedFileMappingValidator(ILog logger, IEnumerable<IValidator<MappingDefinition>> validators)
        {
            var mappingValidators = validators as IValidator<MappingDefinition>[] ?? validators.ToArray();
            Ensure.ThatArgumentIsNotNull(mappingValidators, nameof(validators));
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));

            _logger = logger;
            MappingValidators = mappingValidators;
        }

        public bool Validate(ApiMapping apiMapping, out string errorMessage)
        {
            var isValidMapping = false;            
            errorMessage = string.Empty;
            try
            {
                if (null == apiMapping)
                {
                    errorMessage = "Mapping is null";
                    return false;
                }
                if (apiMapping.IsBatchSupported)
                {
                    return true;
                }
                if (string.IsNullOrWhiteSpace(apiMapping.ObjectType))
                {
                    errorMessage = "Mapping object type cannot be empty.";
                    return false;
                }
                var mappingDefinitions = apiMapping.Mapping;
                foreach (var mappingValidator in MappingValidators)
                {
                    isValidMapping = mappingValidator.Validate(mappingDefinitions, out errorMessage);
                    if (!isValidMapping)
                        break;
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"An Error Occurred in {nameof(MappedFileMappingValidator)}:{nameof(Validate)} ",
                    exception);
            }
            return isValidMapping;
        }
    }
}