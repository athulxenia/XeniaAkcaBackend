using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Models;

namespace XeniaAkcaBackend.Repositories.Districts
{
    public interface IDistrictRepository
    {
        Task<List<District>> GetAllDistrictAsync();
        Task<DistrictResponse> UpdateDistrictAsync(int districtId, UpdateDistrictRequest request);
    }
}