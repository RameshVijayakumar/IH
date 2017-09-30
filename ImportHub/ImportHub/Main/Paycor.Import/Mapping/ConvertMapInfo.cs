using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentValidation;
using FluentValidation.Attributes;

namespace Paycor.Import.Mapping
{
    [ExcludeFromCodeCoverage]
    [Validator(typeof(ConvertMapInfoValidator))]
    public class ConvertMapInfo
    {
        public IEnumerable<ConvertMapKeys> Maps;
    }

    public class ConvertMapInfoValidator : AbstractValidator<ConvertMapInfo>
    {
        public ConvertMapInfoValidator()
        {
            RuleFor(t => t.Maps).Must(t => t.Any()).WithMessage("Atleast one map is needed");
            RuleFor(t => t.Maps).SetCollectionValidator(new ConvertMapKeysValidator());
        }
    }
}
