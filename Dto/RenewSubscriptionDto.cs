namespace XeniaQLaunchBackend.Dto
{
    public class RenewSubscriptionDto
    {
        public int CompanyId { get; set; }
        public int PlanId { get; set; }
        public int PlanDurationId { get; set; }
        public List<int>? AddonPlanIds { get; set; }
    }
}
