namespace XeniaTokenBackend.Dto
{
    public class CompanyTokenDetailDto
    {
        public CompanyTokenListDto Company { get; set; } = null!;
        public CompanyTokenSettingsDto Settings { get; set; } = null!;
    }

    public class CompanyTokenListDto
    {
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool Status { get; set; }

        public string? Country { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }

        public SubscriptionTokenSummaryDto? Subscription { get; set; }
    }

    public class SubscriptionTokenSummaryDto
    {
        public int SubId { get; set; }
        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal Amount { get; set; }
        public int DepCount { get; set; }
    }

    public class CompanyTokenSettingsDto
    {
        public bool CollectCustomerName { get; set; }
        public bool PrintCustomerName { get; set; }

        public bool CollectCustomerMobileNumber { get; set; }
        public bool PrintCustomerMobileNumber { get; set; }

        public bool IsCustomCall { get; set; }
        public bool IsServiceEnable { get; set; }
    }
}
