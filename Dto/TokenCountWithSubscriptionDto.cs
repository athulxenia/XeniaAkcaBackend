namespace XeniaTokenBackend.Dto
{
    public class TokenCountWithSubscriptionDto
    {
        public int PendingCount { get; set; }

        public int CompletedCount { get; set; }

        public bool IsTrial { get; set; }

        public int RemainingDays { get; set; }

        public object? Subscription { get; set; }
    }
}
