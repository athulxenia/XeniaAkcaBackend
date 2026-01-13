using System.Security.Claims;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Company
{
    public interface ICompanyRepository
    {
        Task<int> CreateCompanyAsync(CreateCompanyDto dto);
        Task<int> UpdateCompanyAsync(int companyId, UpdateCompanyDto dto);
        Task<List<xtm_Company>> GetAllCompanyAsync(string search = "");
        Task<xtm_Company?> GetCompanyByIdAsync(int companyId);
        Task<int> UpdateCompanySettingsAsync(int companySettingId, CompanySettingsUpdateDto dto);
        Task<object> GetAllCompanySettingsAsync(int companyId, int userId);

    }
}
