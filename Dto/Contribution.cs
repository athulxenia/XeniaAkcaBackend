namespace XeniaAkcaBackend.Dto
{
    public class CreateContributionRequest
    {
        public int ContributionMemberId { get; set; }
        public string? ContributionImgUrl { get; set; }
        public string? ContributionText { get; set; }
        public string? ContributionContent { get; set; }
        public decimal ContributionAmount { get; set; }
        public int ActiveStatus { get; set; }
        public int ContributionApprovalStatus { get; set; }
        public string? ContributionType { get; set; }
    }

    public class UpdateContributionRequest
    {
        public int ContributionMemberId { get; set; }
        public string? ContributionImgUrl { get; set; }
        public string? ContributionText { get; set; }
        public string? ContributionContent { get; set; }
    }

    public class ApproveContributionRequest
    {
        public bool ActiveStatus { get; set; }
    }

    public class ContributionUpdationRequest
    {
        public string? ContributionChequeNo { get; set; }
        public string? ContributionHandOveredTo { get; set; }
        public string? ContributionContent { get; set; }
        public string? ContributionImgUrl { get; set; }
        public string? ContributionRemarks { get; set; }
        public string? ContributionStatus { get; set; }
    }

    public class PaginationRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public string? SearchText { get; set; }
    }

    public class ContributionResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
    public class ContributionDetailDto
    {
        public int ContributionId { get; set; }
        public string ContributionText { get; set; }
        public string ContributionContent { get; set; }
        public decimal ContributionAmount { get; set; }
        public string ContributionImgUrl { get; set; }
        public DateTime? PaidDate { get; set; } // Keep as DateTime
        public string ContributionDetail { get; set; }
        public string ContributionPaymentRef { get; set; }
        public string PayMode { get; set; }
        public string PaymentStatus { get; set; }
        public string MemberName { get; set; }
        public string MemberBusinessName { get; set; }
        public string DistrictName { get; set; }
        public string UnitName { get; set; }
    }
    public class PaginatedResult<T>
    {
        public List<T> Records { get; set; } = new();
        public int Total { get; set; }
    }
}
