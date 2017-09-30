using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using FluentValidation.Attributes;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{
    [ExcludeFromCodeCoverage]
    [Validator(typeof(CloneMapKeysValidator))]
    public class CloneUserMapping
    {
        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "mappingName")]
        public string MappingName { get; set; }

        [JsonProperty(PropertyName = "cloneTouser")]
        public string CloneToUser { get; set; }


        public class CloneMapKeysValidator : AbstractValidator<CloneUserMapping>
        {
            public CloneMapKeysValidator()
            {
                RuleFor(t => t.MappingName).NotEmpty().WithMessage("Is required.");
                RuleFor(t => t.User).NotEmpty().WithMessage("Is required.");
                RuleFor(t => t.CloneToUser).NotEmpty().WithMessage("Is required.");
            }
        }
    }
}