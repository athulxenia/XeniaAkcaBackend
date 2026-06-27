
namespace XeniaAkcaBackend.Dto
{

    public class ContributionRequest
    {
        public int Status { get; set; } 
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public string? SearchText { get; set; }
        public int? DistrictId { get; set; }
        public int? UnitId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Event { get; set; }
    }

    public class PaymentRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public string? SearchText { get; set; }
        public int? DistrictId { get; set; }
        public int? UnitId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PayType { get; set; }
    }

    public class ReportResponse
    {
        public string Status { get; set; } = "success";
        public object? Data { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int Limit { get; set; }
        public int TotalRecords { get; set; }
        public decimal TotalAmount { get; set; }
    }
}