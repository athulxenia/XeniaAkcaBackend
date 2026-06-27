namespace XeniaAkcaBackend.Dto
{
    public class SettingUpdate
    {
        public int SettingId { get; set; }
        public string? SettingValue { get; set; }
    }

    public class RegistrationPaymentRequest
    {
        public string? PaymentPaymentId { get; set; }
        public string? PaymentOrderId { get; set; }
        public string? PaymentSignature { get; set; }
        public string? PaymentStatus { get; set; }
        public int? PaymentTypeId { get; set; }
    }

    public class ContributionPaymentRequest
    {
        public string? ContributionPaymentId { get; set; }
        public string? ContributionOrderId { get; set; }
        public string? ContributionSignature { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class PaymentInitiateRequest
    {
        public decimal Amount { get; set; }
        public string? Firstname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Zipcode { get; set; }
        public int PaymentTypeId { get; set; }
        public string? PayOpt { get; set; }
        public string? TxnId { get; set; }
        public int? ContributionId { get; set; }
    }

    public class PaymentResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}