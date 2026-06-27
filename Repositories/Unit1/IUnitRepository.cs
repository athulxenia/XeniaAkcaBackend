// Repositories/IUnitRepository.cs

using XeniaAkcaBackend.Dto;

namespace XeniaAkcaBackend.Repositories
{
    public interface IUnitRepository
    {
        // CRUD
        Task<UnitResponse> CreateUnitAsync(UnitRequest request);
        Task<UnitResponse> UpdateUnitAsync(int unitId, UnitRequest request);
        Task<UnitResponse> DeleteUnitAsync(int unitId);

        // Queries
        Task<PaginatedUnitResponse> GetDistrictUnitsAsync(int userId, int page, int limit, string search);
        Task<PaginatedUnitResponse> GetAllStateUnitsAsync(string search, int page, int limit, int districtId);
        Task<PaginatedUnitResponse> GetAllUnitsAsync(string search, int page, int limit);
        Task<List<UnitWithDistrictDto>> GetAllUnitDetailsAsync(int districtId);
        Task<List<UnitWithDistrictDto>> GetUnitMembersAsync();
    }
}