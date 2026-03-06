namespace XeniaQLaunchBackend.Dto
{
    public class RenewSubscriptionResponseDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentLink { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? Message { get; set; }

    }
}
