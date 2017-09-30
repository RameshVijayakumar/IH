using System.Collections.Generic;
using log4net;
using Paycor.Import;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
using Paycor.Import.Web;


namespace EmployeeImportWorker
{
    public class EmployeeImportTranslator : RestApiMappingTranslator
    {
        private readonly ILog _log;
        public EmployeeImportTranslator(ILog log, MappingDefinition mappingDefinition) : base(mappingDefinition)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
            _log.Debug("EmployeeImport Translator initialized.");
        }

        protected override RestApiPayload OnTranslate(FileTranslationData<IDictionary<string, string>> input)
        {
            _log.Debug("Starting translation.");
            var employeeImportFileTranslationData =
                input as EeImportFileTranslationData<IDictionary<string, string>>;

            RestApiPayload result = null;

            if (employeeImportFileTranslationData != null)
            {
                result = new EeImportRestApiPayload()
                {
                    TransactionId = employeeImportFileTranslationData.TransactionId,
                    Name = employeeImportFileTranslationData.Name,
                    Records = employeeImportFileTranslationData.Records,
                    ProcessingStartTime = employeeImportFileTranslationData.ProcessingStartTime,
                    MasterSessionId = employeeImportFileTranslationData.MasterSessionId,
                    MappingValue = employeeImportFileTranslationData.MappingValue
                };
            }
            _log.Debug("Translation complete.");
            return (result);
        }
    }
}