using System;
using System.Collections.Generic;
using System.Linq;
using static Paycor.Import.ImportHubTest.Common.Utils;
namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class MappingField 
    {

        public string Source { get; set; }
        public string Destination { get; set; }
        public string GlobalLookupType { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string EndPoint { get; set; }
        public string ValuePath { get; set; }
        public ImportSourceType SourceType { get; set; }
        public string ExceptionMessage { get; set; }
        public bool IsRequiredForPayload { get; set; }

        public bool Validate()
        {
            List<bool> result = new List<bool>();
            result.Add(ValidateSource());
            result.Add(ValidateDestination());
            result.Add(ValidateType());
            return result.All(x => x);
        }

        public bool ValidateSource(string expected = null)
        {
            return Source.IsNotNullOrEqualToExpectation(expected);
        }

        public bool ValidateDestination(string expected = null)
        {
            return Destination.IsNotNullOrEqualToExpectation(expected);
        }

        public bool ValidateType(string expected = null)
        {
            return Type.IsNotNullOrEqualToExpectation(expected);
        }

        //bool ValidateSourceType<ImportSourceType>(ImportSourceType expected = default(ImportSourceType))
        //{
        //    return SourceType.ValidateDefaultOrExpected<ImportSourceType>(expected);
        //}
    }
}