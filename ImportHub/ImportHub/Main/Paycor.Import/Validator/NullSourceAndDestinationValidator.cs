using System.Linq;
using Paycor.Import.Mapping;

namespace Paycor.Import.Validator
{
    public class NullSourceAndDestinationValidator: IValidator<MappingDefinition>
    {
        public bool Validate(MappingDefinition mappingDefinition, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (mappingDefinition.FieldDefinitions.Any(
                fieldDefinition => string.IsNullOrWhiteSpace(fieldDefinition.Source)))
            {
                errorMessage = "Source field cannot be empty.";
                return false;
            }
            if (mappingDefinition.FieldDefinitions.Any(
                fieldDefinition => string.IsNullOrWhiteSpace(fieldDefinition.Destination)))
            {
                errorMessage = "Destination fields cannot be empty.";
                return false;
            }
            return true;
        }
    }
}
