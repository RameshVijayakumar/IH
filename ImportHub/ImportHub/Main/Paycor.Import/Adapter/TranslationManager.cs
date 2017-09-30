using System;
using System.Collections.Specialized;
using log4net;
using Ninject;

namespace Paycor.Import.Adapter
{
    public class TranslationManager<TInput, TOutput, TResult> : ITranslationManager<TInput, TOutput, TResult>
    {
        #region Fields
        /// <summary>
        /// The importer to use when importing data to the service
        /// </summary>
        IImporter<TInput> _importer;
        /// <summary>
        /// The translator to use when translating the data received
        /// by the importer.
        /// </summary>
        ITranslator<TInput, TOutput> _translator;
        /// <summary>
        /// The exporter to use to export the data translated by the
        /// translator
        /// </summary>
        IExporter<TOutput, TResult> _exporter;

        private ILog _log;
        #endregion

        #region Properties
        /// <summary>
        /// The importer to use when importing data to the service
        /// </summary>
        [Inject]
        public IImporter<TInput> Importer
        {
            get { return (_importer); }
            set { _importer = value; }
        }
        /// <summary>
        /// The translator to use when translating the data received
        /// by the importer.
        /// </summary>
        [Inject]
        public ITranslator<TInput, TOutput> Translator
        {
            get { return (_translator); }
            set { _translator = value; }
        }
        /// <summary>
        /// The exporter to use to export the data translated by the
        /// translator
        /// </summary>
        [Inject]
        public IExporter<TOutput, TResult> Exporter
        {
            get { return (_exporter); }
            set { _exporter = value; }
        }

        [Inject]
        [Obsolete("Use constructor injection instead")]
        public ILog Log
        {
            set { _log = value; }
        }

        #endregion

        #region Contructors

        public TranslationManager(IImporter<TInput> importer,
                                  ITranslator<TInput, TOutput> translator,
                                  IExporter<TOutput, TResult> exporter,
                                  ILog log)
        {
            _importer = importer;
            _translator = translator;
            _exporter = exporter;
            _log = log;
        }
        #endregion

        #region Initialization Routines
        /// <summary>
        /// Initializes the translation manager allowing it to load the
        /// classes that it should use. This method will load and initalize
        /// the importer, translator and exporter that will be used by the 
        /// manager.
        /// </summary>
        /// <returns>
        /// True if initialization was successful; otherwise false.
        /// </returns>
        public virtual bool Initialize(NameValueCollection configSettings = null)
        {
            bool result;
            try
            {
                _log.Debug("Initializing translation manager");

                _log.DebugFormat("TranslationManager: Initializing importer: {0}", _importer.GetType());
                result = _importer.Initialize(configSettings);

                if (result)
                {
                    _log.DebugFormat("TranslationManager: Initializing translator: {0}", _translator.GetType());
                    result = _translator.Initialize(configSettings);
                }
                else
                    _log.DebugFormat("TranslationManager: Importer failed initialization");

                if (result)
                {
                    _log.DebugFormat("TranslationManager: Initializing exporter: {0}", _exporter.GetType());
                    result = _exporter.Initialize(configSettings);
                }
                else
                    _log.DebugFormat("TranslationManager: Translator failed initialization");

                if (result)
                {
                    _importer.ImportComplete += Importer_ImportComplete;
                    _translator.TranslateComplete += Translator_TranslateComplete;
                }
                else
                    _log.DebugFormat("TranslationManager: Exporter failed initialization");
            }
            catch (Exception e)
            {
                _log.Error("An exception occurred while initalizing the translation manager.", e);
                result = false;
            }

            if (result)
                _log.Debug("Translation manager initialized");
            else
                _log.Error("Failed to initialize translation manager");

            return (result);
        }

        void Importer_ImportComplete(object sender, ImporterEventArgs<TInput> e)
        {
            if (null == e.Exception)
                _translator.Translate(e.Data);
        }

        void Translator_TranslateComplete(object sender, TranslatorEventArgs<TOutput> e)
        {
            if (null == e.Exception)
                _exporter.ExportAsync(e.Output);
        }

        #endregion

        /// <summary>
        /// Starts the translation processing, by starting all of the translation 
        /// workers.
        /// </summary>
        /// <returns>
        /// True if all of the workers started otherwise false.
        /// </returns>
        public virtual bool Start()
        {
            var result = _exporter.Start();

            if (result)
                result = _translator.Start();

            if (result)
                result = _importer.Start();

            return (result);

        }

        /// <summary>
        /// Stops the translation processing, by stopping all of the translation 
        /// workers.
        /// </summary>
        /// <returns>
        /// True if all of the workers stopped otherwise false.
        /// </returns>
        public virtual bool Stop()
        {
            var result = _importer.Stop();
            result &= _translator.Stop();
            result &= _exporter.Stop();

            return (result);
        }

    }
}
