using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Validator;

namespace Paycor.Import.MapFileImport.Implementation.Transformers
{
    public class LookupRouteFieldTransformer : NullMappingDefinitionValidator, ITransformRecordFields<MappingDefinition>
    {
        private readonly ILog _log;
        private readonly ILookupResolver<MappingFieldDefinition> _lookupResolver;
        private readonly IRulesEvaluator _lookupRulesEvaluator;

        public LookupRouteFieldTransformer(ILog log, ILookupResolver<MappingFieldDefinition> lookupResolver, IRulesEvaluator rulesEvaluator)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(lookupResolver, nameof(lookupResolver));
            Ensure.ThatArgumentIsNotNull(rulesEvaluator, nameof(rulesEvaluator));

            _log = log;
            _lookupResolver = lookupResolver;
            _lookupRulesEvaluator = rulesEvaluator;
        }

        public IEnumerable<KeyValuePair<string, string>> TransformRecordFields(MappingDefinition mappingDefinition, 
            string masterSessionId, IDictionary<string, string> record = null, 
            IEnumerable<KeyValuePair<string, string>> recordKeyValuePairs = null,
            ILookup lookup = null)
        {
            var exceptionMessages = new List<string>();
            string errorMessage;

            var recordFields = new List<KeyValuePair<string, string>>();

            if (null == recordKeyValuePairs || !Validate(mappingDefinition, out errorMessage))
            {
                return recordFields;
            }

            var recordColumnsValues = recordKeyValuePairs.ToList();

            var lookupDefinitionOrder = _lookupRulesEvaluator.SortLookupOrder(recordColumnsValues, mappingDefinition, new FieldDefinitionComparer());

            foreach (var mappingFieldDefinition in lookupDefinitionOrder)
            {
                if (!_lookupRulesEvaluator.ValidateRules(mappingFieldDefinition, recordFields,
                    recordColumnsValues.Select(t=>t.Key).ToList().Distinct()
                    )) continue;

                var lookupValues = _lookupResolver.RetrieveLookupValue(mappingFieldDefinition,
                       masterSessionId, recordColumnsValues,lookup);
                AddLookedUpKeyValuePair(lookupValues, recordFields, mappingFieldDefinition, exceptionMessages);
                if (string.IsNullOrWhiteSpace(lookupValues?.Lookupvalue)) continue;
                _log.Debug($"Lookup Transformed Field:{mappingFieldDefinition.Destination.RemoveBraces()} Value:{lookupValues?.Lookupvalue}");
                recordColumnsValues = recordColumnsValues.AddOrOverwriteKeyValuePair(
                                      new KeyValuePair<string, string>(mappingFieldDefinition.Destination.RemoveBraces(), lookupValues.Lookupvalue));
            }

            var transformedFields = recordColumnsValues.GetRecordsWithFormattedExceptionMessage(exceptionMessages);
            return transformedFields;
        }

        private static void AddLookedUpKeyValuePair(Lookupvalues lookupvalues, 
            ICollection<KeyValuePair<string, string>> recordFields, MappingFieldDefinition mappingFieldDefinition,
            ICollection<string> exceptionMessages)
        {
            if (!string.IsNullOrWhiteSpace(lookupvalues?.Lookupvalue))
            {
                recordFields.Add(new KeyValuePair<string, string>(mappingFieldDefinition.Destination.RemoveBraces(), lookupvalues.Lookupvalue));
            }
            else
            {
                if (!exceptionMessages.Contains(mappingFieldDefinition.ExceptionMessage))
                {
                    exceptionMessages.Add(mappingFieldDefinition.ExceptionMessage.ConcatLookupValuesToLookupMessage(lookupvalues?.LookupParameterValues));
                }
            }
        }
    }
}