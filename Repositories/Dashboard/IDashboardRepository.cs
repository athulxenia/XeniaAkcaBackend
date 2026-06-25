using XeniaAkcaBackend.Models;


namespace XeniaAkcaBackend.Repositories.Dashboard
{
    public interface IDashboardRepository
    {
        Task<object> GetDashboardAsync(string type, int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate);
    }
}