namespace XeniaAkcaBackend.Repositories
{
    public interface IDashboardRepository
    {
        Task<object> GetDashboardAsync(int userId, int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate);
    }
}