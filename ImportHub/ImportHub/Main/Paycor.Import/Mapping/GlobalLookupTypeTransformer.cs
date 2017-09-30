using System;
using System.Collections.Generic;
using Paycor.Import.Validator;

//TODO: No unit tests

namespace Paycor.Import.Mapping
{
    public class GlobalLookupTypeTransformer : NullMappingDefinitionValidator, ITransformFields<MappingDefinition>
    {
        private readonly IGlobalLookupTypeReader _globalLookupTypeReader;

        public GlobalLookupTypeTransformer(IGlobalLookupTypeReader globalLookupTypeReader)
        {
            _globalLookupTypeReader = globalLookupTypeReader;
        }
        public IDictionary<string, string> TransformFields(MappingDefinition mappingDefinition, IDictionary<string, string> record,string masterSessionId = null)
        {
            var result = new Dictionary<string, string>();
            string errorMessage;

            if (!Validate(mappingDefinition, out errorMessage))
                return result;

            foreach (var mappingFieldDefinition in mappingDefinition.FieldDefinitions)
            {
                if (!string.IsNullOrWhiteSpace(mappingFieldDefinition.EndPoint))
                    continue;

                if (mappingFieldDefinition.Source == null || string.IsNullOrWhiteSpace(mappingFieldDefinition.GlobalLookupType))
                    continue;

                try
                {
                    var columnValue = SourceTypeHandlerFactory.HandleSourceType(mappingFieldDefinition).Resolve(mappingFieldDefinition, mappingFieldDefinition.Source, record);

                    var globalLookup = _globalLookupTypeReader.LookupDefinition(mappingFieldDefinition.GlobalLookupType);

                    if (globalLookup == null)
                        continue;

                    string lookupTypeValue;
                    if (columnValue != null && (globalLookup.LookupTypeValue != null && globalLookup.LookupTypeValue.TryGetValue(columnValue, out lookupTypeValue)))
                    {
                        result[mappingFieldDefinition.Destination] = lookupTypeValue;
                    }
                }
                catch (Exception e)
                {
                    var transformerException = new TransformerException(e.ToString())
                    {
                        Hint = "Exception occured during transformation of a record!",
                        FailedSourcePropertyName = mappingFieldDefinition.Source,
                        FailedDestinationPropertyName = mappingFieldDefinition.Destination,
                        FailedRecord = record
                    };
                    throw transformerException;
                }
            }
            return result;
        }
    }
}