using XeniaAkcaBackend.Dto;

namespace XeniaAkcaBackend.Repositories.Informations
{
    public interface IInformationRepository
    {
        Task<InformationResponse> CreateInformationAsync(CreateInformationRequest request);
        Task<InformationResponse> UpdateInformationAsync(int informationId, UpdateInformationRequest request);
        Task<List<object>> GetInformationByPartialNameAsync(string partialName);
        Task<(List<object> Records, int Total)> GetStateInformationAsync(InformationListRequest request);
        Task<InformationResponse?> ApproveInformationAsync(int informationId, bool activeStatus);
        Task<List<object>> GetInformationDetailsAsync(int districtId);
    }
}