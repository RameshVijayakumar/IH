using FluentValidation;
using FluentValidation.Attributes;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{
    [Validator(typeof(CommonSaveMappingValidator<CommonSavedMapping>))]
    public class CommonSavedMapping : ApiMapping
    {
        public class CommonSaveMappingValidator<T> : AbstractValidator<T> where T:CommonSavedMapping
        {
            public CommonSaveMappingValidator()
            {
                RuleFor(x => x.Mapping)
                    .NotEmpty();
                RuleFor(x => x.MappingName)
                    .NotEmpty();
                RuleFor(x => x.MappingEndpoints)
                    .NotEmpty();
                RuleFor(x => x.Mapping).SetValidator(new MappingDefinitionValidator());
            }
        }

        public class MappingDefinitionValidator : AbstractValidator<MappingDefinition>
        {
            public MappingDefinitionValidator()
            {
                RuleFor(x => x.FieldDefinitions)
                    .NotEmpty();
            }
        }

        [JsonProperty(PropertyName = "isMappingValid")]
        public bool? IsMappingValid { get; set; }

    }
}
