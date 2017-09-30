using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentValidation;
using FluentValidation.Attributes;
using Newtonsoft.Json;


namespace Paycor.Import.Mapping
{
    [Validator(typeof(DeleteMapInfoValidator))]
    [ExcludeFromCodeCoverage]
    public class DeleteMapInfo
    {
        public IEnumerable<DeleteMapKeys> Maps;
    }
    public class DeleteMapInfoValidator : AbstractValidator<DeleteMapInfo>
    {
        public DeleteMapInfoValidator()
        {
            RuleFor(t => t.Maps).Must(t => t.Any()).WithMessage("Atleast one map is needed");
            RuleFor(t => t.Maps).SetCollectionValidator(new DeleteMapKeysValidator());
        }
    }

    [ExcludeFromCodeCoverage]
    public class DeleteMapKeys
    {
        [JsonProperty(PropertyName = "mappingName")]
        public string MappingName { get; set; }

        [JsonProperty(PropertyName = "mapType")]
        public MapType? MapType;

        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class DeleteMapKeysValidator : AbstractValidator<DeleteMapKeys>
    {
        public DeleteMapKeysValidator()
        {
            RuleFor(t => t.MappingName).NotEmpty().WithMessage("MappingName is required.");
            RuleFor(t => t.MapType).NotEmpty().WithMessage("MapType is required. Valid Values for MapType is Global,Client,User");
            RuleFor(t => t.MapType)
                .IsInEnum()
                .WithMessage("MapType is invalid.Valid Values for MapType is Global,Client,User");
            RuleFor(t => t.ClientId)
                .NotEmpty()
                .When(t => t.MapType == MapType.Client)
                .WithMessage("ClientId is required.");
            RuleFor(t => t.ClientId)
                .Must(t =>
                    {
                        int x;
                        return int.TryParse(t, out x);
                    })
                .When(t => t.MapType == MapType.Client)
               .WithMessage("Must be an integer value.");
            RuleFor(t => t.User)
                .NotEmpty()
                .When(t => t.MapType == MapType.User)
                .WithMessage("User is required.");
        }
    }
}
