using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using FluentValidation.Attributes;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{
    [ExcludeFromCodeCoverage]
    [Validator(typeof(CloneClientMappingValidator))]
    public class CloneClientMapping 
    {
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "mappingName")]
        public string MappingName { get; set; }

        [JsonProperty(PropertyName = "cloneToclientId")]
        public string CloneToClientId { get; set; }


        public class CloneClientMappingValidator : AbstractValidator<CloneClientMapping>
        {
            public CloneClientMappingValidator()
            {
                RuleFor(t => t.MappingName).NotEmpty().WithMessage("Is required.");
                RuleFor(t => t.ClientId).NotEmpty().WithMessage("Is required.");
                RuleFor(t => t.ClientId).Must(t =>
                {
                    int x;
                    return int.TryParse(t, out x);
                }).WithMessage("Must be an integer value.");
                RuleFor(t => t.CloneToClientId).NotEmpty().WithMessage("Is required.");
                RuleFor(t => t.CloneToClientId).Must(t =>
                {
                    int x;
                    return int.TryParse(t, out x);
                }).WithMessage("Must be an integer value.");
            }
        }
    }
}