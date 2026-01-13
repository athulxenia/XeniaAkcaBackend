using System.Security.Claims;
using XeniaTokenBackend.Dto;

namespace XeniaTokenBackend.Repositories.Advertisement
{
    public interface IAdvertisementRepository
    {
        Task<object> CreateAdvertisementAsync(CreateAdvertisementDto dto);
        Task<List<AdvertisementDto>> GetCompanyAdvertisementsAsync(int companyId);
        Task<AdvertisementResponseDto> GetAdvertisementsByUserAsync(int userId);
        Task<object> UpdateAdvertisementAsync(int advId, UpdateAdvertisementDto dto);
        Task<object> DeleteAdvertisementAsync(int advId);

    }
}
