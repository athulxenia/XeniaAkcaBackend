namespace XeniaTempleBackend.Dtos
{
    public class PlanDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PlanDescription { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public int PlanDurationDays { get; set; }
        public int PlanDeps { get; set; }
        public bool PlanIsAddOn { get; set; }
        public int PlanExpireDays { get; set; }
        public bool PlanActive { get; set; }
        public List<PlanDurationDto> Durations { get; set; } = new();
    }


    public class PlanDurationDto
    {
        public int PlanDurationId { get; set; }
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
    }
}
