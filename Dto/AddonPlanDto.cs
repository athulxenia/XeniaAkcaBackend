namespace XeniaQLaunchBackend.Dto
{
    public class AddonPlanDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public int PlanDeps { get; set; }
    }

}
