using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public interface IApiMappingStatusService
    {
        ApiMappingStatusInfo GetApiStatusInfo(string docUrl);

        ApiMappingStatusInfo CreateApiStatusInfo(string docUrl);

        void UpdateApiStatusInfo(ApiMappingStatusInfo apiMappingStatusInfo);

        bool IsRecentlyProcessed(ApiMappingStatusInfo apiMappingStatusInfo);

        int RegistrationThreshold { get; }
    }
}