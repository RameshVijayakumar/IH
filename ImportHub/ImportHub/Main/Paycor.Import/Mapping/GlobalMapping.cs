using System.Diagnostics.CodeAnalysis;
using FluentValidation.Attributes;

namespace Paycor.Import.Mapping
{
    [ExcludeFromCodeCoverage]
    [Validator(typeof(GlobalMappingValidator))]
    public class GlobalMapping : CommonSavedMapping
    {
        public class GlobalMappingValidator : CommonSaveMappingValidator<GlobalMapping>
        {
            public GlobalMappingValidator()
            {
            }
        }
    }
}
