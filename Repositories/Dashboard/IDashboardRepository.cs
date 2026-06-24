using XeniaAkcaBackend.Models;


namespace XeniaAkcaBackend.Repositories.Dashboard
{
    public interface IDashboardRepository
    {
        Task<DashboardResponse> GetAllStateWiseDetailsAsync(int? districtId, string? dateId, DateTime? fromDate, DateTime? toDate);
        Task<DashboardResponse> GetAllDistrictWiseDetailsAsync(int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate);
        Task<DashboardResponse> GetAllUnitWiseDetailsAsync(int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate);
        Task<DashboardResponse> GetAllStateWiseGraphDetailsAsync(int? districtId, string? dateId, DateTime? fromDate, DateTime? toDate);
        Task<DashboardResponse> GetAllDistrictWiseGraphDetailsAsync(int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate);
        Task<DashboardResponse> GetAllStateWiseDetailsAndGraphAsync(int? districtId, string? dateId, DateTime? fromDate, DateTime? toDate);
        Task<DashboardResponse> GetAllDistrictWiseDetailsAndGraphAsync(int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate);
    }
}