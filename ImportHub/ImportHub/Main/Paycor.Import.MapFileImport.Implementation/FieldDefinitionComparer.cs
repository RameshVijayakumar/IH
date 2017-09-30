using System;
using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation
{
    public class FieldDefinitionComparer : IEqualityComparer<MappingFieldDefinition>
    {
       
        public bool Equals(MappingFieldDefinition x, MappingFieldDefinition y)
        {
            return string.Equals(x?.EndPoint, y?.EndPoint, StringComparison.OrdinalIgnoreCase)
                   &&
                   string.Equals(x?.Destination, y?.Destination, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(MappingFieldDefinition obj)
        {
            return obj.GetHashCode();
        }
    }
}