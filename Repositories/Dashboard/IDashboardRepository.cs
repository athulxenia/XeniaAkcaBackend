
using XeniaTokenBackend.Dto;

namespace XeniaTokenBackend.Repositories.Dashboard
{
    public interface IDashboardRepository
    {

        Task<TokenDashboardDto> GetTokenValuesAsync(int companyId);


    }
}
