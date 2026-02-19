using System.Security.Claims;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Company
{
    public interface ICompanyRepository
    {
        Task<int> UpdateCompanyAsync(int companyId, UpdateCompanyDto dto);
        Task<CompanyTokenDetailDto?> GetCompanyByIdAsync(int companyId);
        Task<int> UpdateCompanySettingsAsync(int companySettingId, CompanySettingsUpdateDto dto);
        Task<object> GetAllCompanySettingsAsync(int companyId, int userId);

    }
}
