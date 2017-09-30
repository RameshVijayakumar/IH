using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Transformers
{
    public class MappedFileRecordTransformer : ITransformRecord<MappingDefinition>
    {
        private readonly ILog _logger;

        public IEnumerable<ITransformFields<MappingDefinition>> FieldTransformers { get; private set; }

        public MappedFileRecordTransformer(ILog logger, ICollection<ITransformFields<MappingDefinition>> fieldTranformers)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
            Ensure.ThatArgumentIsNotNull(fieldTranformers, nameof(fieldTranformers));

            _logger = logger;
            FieldTransformers = fieldTranformers;
        }

        public IDictionary<string, string> TransformRecord(MappingDefinition mappingDefinition, IDictionary<string, string> record, string masterSessionId)
        {
            var transformedResult = new Dictionary<string, string>();
            var listofDictionaries = new List<IDictionary<string, string>>();
            try
            {
                foreach (var fieldtransformer in FieldTransformers)
                {
                    var result = fieldtransformer.TransformFields(mappingDefinition, record, masterSessionId);
                    listofDictionaries.Add(result);
                    record = result;
                }
                foreach (var pair in listofDictionaries.SelectMany(dictionary => dictionary))
                {
                    transformedResult[pair.Key] = pair.Value;
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"An Error Occurred in {nameof(MappedFileRecordTransformer)}:{nameof(TransformRecord)} ", exception);
            }

            return transformedResult;
        }

    }
}
