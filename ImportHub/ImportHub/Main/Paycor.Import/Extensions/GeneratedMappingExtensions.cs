using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Mapping;

namespace Paycor.Import.Extensions
{
    public static class GeneratedMappingExtensions
    {
        public static GeneratedMapping RemoveActionFieldFromMap(this GeneratedMapping mapping)
        {
            var fieldDefinition = mapping.Mapping.FieldDefinitions.Where(t => t.Destination.Equals(ImportConstants.ActionFieldName, StringComparison.CurrentCultureIgnoreCase)).Select(t => t).FirstOrDefault();
            var coll = (ICollection<MappingFieldDefinition>)mapping.Mapping.FieldDefinitions;
            coll.Remove(fieldDefinition);
            mapping.Mapping.FieldDefinitions = coll;

            return mapping;
        }
    }
}
