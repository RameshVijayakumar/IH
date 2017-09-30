using System;
using System.Configuration;
using Paycor.Import.Azure;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public class ApiMappingStatusService : IApiMappingStatusService
    {
        private readonly IDocumentDbRepository<ApiMappingStatusInfo> _registrationStatusRepository;
        private const int DefaultRegistrationThreshold = 5;

        public ApiMappingStatusService(IDocumentDbRepository<ApiMappingStatusInfo> registrationStatusRepository)
        {
            Ensure.ThatArgumentIsNotNull(registrationStatusRepository, nameof(registrationStatusRepository));
            _registrationStatusRepository = registrationStatusRepository;
            RegistrationThreshold = GetRegistrationThreshold();
        }

        public int RegistrationThreshold { get; private set; }

        public void UpdateApiStatusInfo(ApiMappingStatusInfo apiMappingStatusInfo)
        {
            Ensure.ThatArgumentIsNotNull(apiMappingStatusInfo, nameof(apiMappingStatusInfo));
            _registrationStatusRepository.UpdateItemAsync(apiMappingStatusInfo.Id, apiMappingStatusInfo);
        }

        public ApiMappingStatusInfo CreateApiStatusInfo(string docUrl)
        {
            Ensure.ThatStringIsNotNullOrEmpty(docUrl, nameof(docUrl));
            var apiMappingStatusInfo = new ApiMappingStatusInfo
            {
                DocUrl = docUrl
            };
            var doc = _registrationStatusRepository.CreateItemAsync(apiMappingStatusInfo).Result;
            apiMappingStatusInfo.Id = doc.Id;
            return apiMappingStatusInfo;
        }

        public ApiMappingStatusInfo GetApiStatusInfo(string docUrl)
        {
            Ensure.ThatStringIsNotNullOrEmpty(docUrl, nameof(docUrl));
            var apiStatusInfo = _registrationStatusRepository.GetItem(reg => reg.DocUrl.ToLower() == docUrl.ToLower());
            return apiStatusInfo;
        }

        public bool IsRecentlyProcessed(ApiMappingStatusInfo apiMappingStatusInfo)
        {
            if (apiMappingStatusInfo?.LastRegistered == null) return false;

            var span = DateTime.Now.Subtract(apiMappingStatusInfo.LastRegistered.Value);
            return span.TotalMinutes < RegistrationThreshold;
        }

        private int GetRegistrationThreshold()
        {
            int result;
            return int.TryParse(ConfigurationManager.AppSettings["StaleRegistrationThreshold"], out result) ? result : DefaultRegistrationThreshold;
        }
    }
}