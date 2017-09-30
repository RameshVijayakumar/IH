using System;

namespace Paycor.Import.Mapping
{
    public class MappingFieldDefinition : IComparable<MappingFieldDefinition>
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public string GlobalLookupType { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string EndPoint { get; set; }
        public string ValuePath { get; set; }
        public SourceTypeEnum SourceType { get; set; }
        public string ExceptionMessage { get; set; }
        public bool IsRequiredForPayload { get; set; }
        public string HeadingDestination { get; set; }

        public int CompareTo(MappingFieldDefinition other)
        {
            if (Source.Contains(other.Destination) || EndPoint.Contains(other.Destination))
                return 1;
            return -1;
        }
        
    }
}