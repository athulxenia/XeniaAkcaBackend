namespace XeniaAkcaBackend.Models
{

    public class DashboardRequest
    {
        public int? DistrictId { get; set; }
        public int? UnitId { get; set; }
        public string? DateId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class MembershipStats
    {
        public int NewMemberships { get; set; }
        public int PendingMemberships { get; set; }
        public int PendingDistrictLevel { get; set; }
    }

    public class DashboardResponse
    {
        public string Status { get; set; } = "success";
        public object? Data { get; set; }
    }
}
