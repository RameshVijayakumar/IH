using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public class MappingCertification : IMappingCertification
    {
        #region Fields

        private readonly ILog _log;
        private readonly ISwaggerParser _swaggerParser;
        private readonly IMappingsNonPostInfo _mappingNonPostInfo;
        private readonly IEnumerable<IVerifyMaps> _verifyMaps;

        #endregion

        public MappingCertification(ILog log, ISwaggerParser swaggerParser, IMappingsNonPostInfo mappingLoggingRules,
            ICollection<IVerifyMaps> verifyMaps)
        {
            Ensure.ThatArgumentIsNotNull(swaggerParser, nameof(swaggerParser));
            Ensure.ThatArgumentIsNotNull(mappingLoggingRules, nameof(mappingLoggingRules));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(verifyMaps, nameof(verifyMaps));
            _log = log;
            _mappingNonPostInfo = mappingLoggingRules;
            _swaggerParser = swaggerParser;
            _verifyMaps = verifyMaps;

        }
        public bool IsAllMappingCertified(string swaggerText)
        {
            try
            {
                if (swaggerText == null || swaggerText == ImportConstants.EmptyStringJsonArray)
                    return false;

                _mappingNonPostInfo.LogAllNonOptInPostMapNames(swaggerText);
                var apiMappings = _swaggerParser.GetAllApiMappings(swaggerText).ToList();
                if (!apiMappings.Any())
                {
                    _log.Warn("No mappings generated.");
                    return false;
                }

                var allMappings = apiMappings;
                var mappings = new List<GeneratedMapping>();
                foreach (var verifyMap in _verifyMaps)
                {
                    mappings = verifyMap.CertifyMaps(apiMappings).ToList();
                    apiMappings = mappings;
                }
                return allMappings.Count == mappings.Count;
            }
            catch (Exception ex)
            {
                _log.Fatal("Error occurred while certifying the swagger Url.", ex);
                return false;
            }
        }
    }
}
