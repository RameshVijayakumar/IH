using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using FluentValidation.Attributes;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{
    [ExcludeFromCodeCoverage]
    [Validator(typeof(ClientMappingValidator))]
    public class ClientMapping : CommonSavedMapping
    {
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

        public class ClientMappingValidator : CommonSaveMappingValidator<ClientMapping>
        {
            public ClientMappingValidator()
            {
                RuleFor(cm => cm.ClientId).NotEmpty().WithMessage("Is required.");
                RuleFor(cm => cm.ClientId).Must(cid =>
                {
                    int x;
                    return int.TryParse(cid, out x);
                }).WithMessage("Must be an integer value.");
            }
        }
    }
}
