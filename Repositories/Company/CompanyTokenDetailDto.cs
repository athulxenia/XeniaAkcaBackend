using XeniaTempleBackend.Dtos;
using XeniaTokenBackend.Dto;

namespace XeniaTokenBackend.Repositories.Company
{
    public class CompanyTokenDetailDto
    {
        public int ActiveDepCount { get; set; }
        public CompanyTokenListDto Company { get; set; } = null!;
        public CompanyTokenSettingsDto Settings { get; set; } = null!;
        public SubscriptionDto? Subscription { get; set; }
        public PlanDto? Plan { get; set; }
        public List<SubscriptionAddonDto> Addons { get; set; } = new();
    }

    public class SubscriptionDto
    {
        public int SubId { get; set; }
        public int PlanId { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public decimal SubscriptionAmount { get; set; }
        public int SubscriptionDays { get; set; }
        public int SubscriptionDepCount { get; set; }
        public int SubscriptionExpireDays { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SubscriptionAddonDto
    {
        public int SubAddonId { get; set; }
        public decimal Amount { get; set; }
        public int DepCount { get; set; }
    }
}