namespace XeniaTokenBackend.Dto
{
    public class TokenDashboardDto
    {
        public int Pending { get; set; }
        public int Completed { get; set; }
        public int DepCount { get; set; }
        public int AllowedDepCount { get; set; }
        public string? SubscriptionStatus { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public int RemainingDays { get; set; }
    }
}
