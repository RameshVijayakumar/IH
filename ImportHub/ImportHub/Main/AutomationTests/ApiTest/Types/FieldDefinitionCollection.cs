using System.Collections.Generic;
using System.Linq;

namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class FieldDefinitionCollection
    {
        public IEnumerable<MappingField> FieldDefinitions { get; set; }

        public bool Validate()
        {
            List<bool> result = new List<bool>();
            foreach (var fieldDefinition in FieldDefinitions)
            {
                result.Add(fieldDefinition.ValidateType());
            }
            return result.All(x => x);
        }
    }
}