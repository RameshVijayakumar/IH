using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{
    [ExcludeFromCodeCoverage]
    public class ConvertMapKeys
    {
        [JsonProperty(PropertyName = "currentMappingName")]
        public string CurrentMappingName { get; set; }

        [JsonProperty(PropertyName = "convertToMappingName")]
        public string ConvertToMappingName { get; set; }

        [JsonProperty(PropertyName = "currentMapType")]
        public MapType CurrentMapType;

        [JsonProperty(PropertyName = "convertToMapType")]
        public MapType ConvertToMapType;

        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }
    }
    public class ConvertMapKeysValidator : AbstractValidator<ConvertMapKeys>
    {
        public ConvertMapKeysValidator()
        {
            RuleFor(t => t.CurrentMappingName).NotEmpty().WithMessage("CurrentMappingName is required.");
            RuleFor(t => t.ConvertToMappingName).NotEmpty().WithMessage("ConvertToMappingName is required.");
            RuleFor(t => t.CurrentMapType).NotEmpty().WithMessage("CurrentMapType is required. Valid Values for MapType is Global,Client,User.");
            RuleFor(t => t.ConvertToMapType).NotEmpty().WithMessage("ConvertToMapType is required. Valid Values for MapType is Global,Client,User.");
            RuleFor(t => t.CurrentMapType)
                .IsInEnum()
                .WithMessage("CurrentMapType is invalid.");
            RuleFor(t => t.ConvertToMapType)
                .IsInEnum()
                .WithMessage("ConvertToMapType is invalid.");
            RuleFor(t => t.ClientId)
                .NotEmpty()
                .When(t => t.ConvertToMapType == MapType.Client)
                .WithMessage("ClientId is required.");
            RuleFor(t => t.ClientId)
                .Must(t =>
                {
                    int x;
                    return int.TryParse(t, out x);
                })
                .When(t => t.ConvertToMapType == MapType.Client)
                .WithMessage("Must be an integer value.");
        }
    }
}