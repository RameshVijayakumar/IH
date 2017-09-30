using Paycor.Import.Mapping;

namespace Paycor.Import.Validator
{
    public class NullMappingDefinitionValidator : IValidator<MappingDefinition>
    {
        public bool Validate(MappingDefinition mappingDefinition, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!IsNullMappingDefinition(mappingDefinition))
            {
                return true;
            }
            errorMessage = "Either the mapping definition or the field definitions array is null.";
            return false;
        }

        private static bool IsNullMappingDefinition(MappingDefinition mappingDefinition)
        {
            return mappingDefinition?.FieldDefinitions == null;
        }
    }
}