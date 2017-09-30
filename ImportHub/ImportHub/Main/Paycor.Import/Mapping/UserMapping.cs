using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using FluentValidation;
using FluentValidation.Attributes;

namespace Paycor.Import.Mapping
{
    [ExcludeFromCodeCoverage]
    [Validator(typeof(UserMapping))]
    public class UserMapping : CommonSavedMapping
    {
        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        public class UserMappingValidator : CommonSaveMappingValidator<UserMapping>
        {
            public UserMappingValidator()
            {
                RuleFor(x => x.User).NotEmpty();
            }
        }
    }
}
