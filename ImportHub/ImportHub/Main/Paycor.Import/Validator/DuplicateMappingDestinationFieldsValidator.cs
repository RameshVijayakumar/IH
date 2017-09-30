using System;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

//TODO: No unit tests

namespace Paycor.Import.Validator
{
    public class DuplicateMappingDestinationFieldsValidator: IValidator<MappingDefinition>
    {
        public bool Validate(MappingDefinition mappingDefinition, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!mappingDefinition.FieldDefinitions.ContainDuplicates(o => o.Destination, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            errorMessage = "A duplicate field definition exists. Please check the destination property of each field definition and remove any duplicate values.";
            return false;
        }
    }
}