using XeniaAkcaBackend.Dto;

namespace XeniaAkcaBackend.Repositories.Advertisements
{
    public interface IAdvertisementRepository
    {
        Task<AdvertisementResponse> CreateAsync(CreateAdvertisementRequest request);
        Task<AdvertisementResponse> UpdateAsync(int advertisementId, UpdateAdvertisementRequest request);
        Task<List<object>> SearchAsync(string partialName);
        Task<object> GetStateAsync(int page, int limit, string? searchText, int? districtId, DateTime? fromDate, DateTime? toDate, int? advertisementId);
        Task<AdvertisementResponse> ApproveAsync(int advertisementId, bool activeStatus);
        Task<List<object>> GetListAsync(int districtId);
    }
}
