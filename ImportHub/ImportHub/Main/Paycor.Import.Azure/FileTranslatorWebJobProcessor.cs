using System;
using System.Linq;
 using System.Threading.Tasks;
using log4net;
using Paycor.Import.Adapter;
using Paycor.Import.Messaging;



namespace Paycor.Import.Azure
{
    public class FileTranslatorWebJobProcessor<TFileData, TPayload, TExporterResult, TMessage>
        : IWebJobProcessor<TMessage> where TPayload : IntegrationsPayload
        where TExporterResult : ExporterResult
        where TMessage : FileUploadMessage
    {
        #region Fields

        private bool _initialized;

        private readonly TranslationManager<FileTranslationData<TFileData>,
            TPayload,
            TExporterResult> _translationManager;

        private readonly ILog _log;

        #endregion

        #region Constructors

        // ReSharper disable once PublicConstructorInAbstractClass
        public FileTranslatorWebJobProcessor(TranslationManager<FileTranslationData<TFileData>,
            TPayload,
            TExporterResult> translationManager, ILog log)
        {
            _translationManager = translationManager;
            _log = log;
        }

        #endregion

        public void Process(TMessage message)
        {
            if (!_initialized)
            {
                Initialize();
            }

            _log.Debug("Processing message");

            MessageReceivedCallback(message);
        }

        public async Task ProcessAsync(TMessage message)
        {
            if (!_initialized)
            {
                Initialize();
            }

            _log.Debug("Processing message");

            await MessageReceivedCallbackAsync(message);
        }

        protected virtual async Task MessageReceivedCallback(TMessage message)
        {
            try
            {
                _log.InfoFormat("{0}: Received from message queue, starting import process.", message);
                await _translationManager.Importer.ImportAsync(message);
            }
            catch (Exception e)
            {
                _log.Error($"An exception occurred while processing {message.File} file upload message. The file has not processed and will need to be resubmitted.", e);
            }

        }

        protected virtual async Task MessageReceivedCallbackAsync(TMessage message)
        {
            try
            {
                _log.InfoFormat("{0}: Received from message queue, starting import process.", message);
                await _translationManager.Importer.ImportAsync(message);
            }
            catch (Exception e)
            {
                _log.Error($"An exception occurred while processing {message.File} file upload message. The file has not processed and will need to be resubmitted.", e);
            }

        }

        protected virtual void Initialize()
        {
            _log.Debug("Initializing FileLoaderWebJob");

            _translationManager.Initialize();
            _translationManager.Importer.ImportComplete += Importer_ImportComplete;
            _translationManager.Translator.TranslateComplete += Translator_TranslateComplete;
            _translationManager.Exporter.ExportComplete += Exporter_ExportComplete;

            _initialized = true;
            _log.Debug("Initialized FileLoaderWebJob");
        }

        void Importer_ImportComplete(object sender, ImporterEventArgs<FileTranslationData<TFileData>> importerEventArgs)
        {
            if (null == importerEventArgs.Exception)
            {
                _log.InfoFormat("{0}: {1} rows have been imported and are being passed to the translator.",
                    importerEventArgs.Data.Name, importerEventArgs.Data.Records.Count());
            }

            else
            {
                _log.Error("Import failed!", importerEventArgs.Exception);
            }
        }

        void Translator_TranslateComplete(object sender, TranslatorEventArgs<TPayload> e)
        {
            if (null == e.Exception)
            {
                _log.InfoFormat("{0}: {1} records have been translated.",
                    e.Output.Name, e.Output.RecordCount);
            }
            else
            {
                _log.Error("Translation failed!", e.Exception);
            }

        }

        void Exporter_ExportComplete(object sender, ExporterEventArgs<TPayload, TExporterResult> e)
        {
            if ((null == e.Exception) &&
                (null == e.Result.Exception))
            {
                if (e.Result.IsSuccess)
                {
                    _log.InfoFormat(
                        "{0}: {1} records have been successfully exported.",
                        e.Data.Name,
                        e.Data.RecordCount);
                }
                else
                {
                    _log.ErrorFormat("{0}: {1} records failed to export reason :{2}!",
                        e.Data.Name,
                        e.Data.RecordCount,
                        e.Result.Result);
                }
            }
            else
            {
                _log.Error("An exception occurred during export, export failed!",
                    e.Exception ?? e.Result.Exception);
            }
        }
    }
}
