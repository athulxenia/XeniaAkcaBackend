

using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Department
{
    public interface IDepartmentRepository
    {
        Task<List<DepartmentDto>> GetDepartmentWebByIdAsync(int userId);
        Task<List<DepartmentWithTokenDto>> GetDepartmentWebAll(int companyId);
        Task<object> CreateDepartmentAsync(CreateDepartmentRequestDto dto);
        Task<object> UpdateDepartmentAsync(int depId, UpdateDepartmentRequestDto dto);
        Task<List<xtm_Department>> GetAllDepartmentsAsync(string? depNameSearch = null);
        Task<object> GetAllDepartmentsByCompanyAsync(int companyId);
        Task<DepartmentResponseDto> GetAllDepartmentsAppByUserIdAsync(int userId);
        Task<int> DeleteDepartmentAsync(int depId);
        Task<int> CreateDepartmentAsync(CreateDepartmentDto dto);

    }
}
