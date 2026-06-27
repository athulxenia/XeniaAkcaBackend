
namespace XeniaAkcaBackend.Dto
{

    public class PaymentSettingDto
    {
        public int SettingId { get; set; }
        public string? SettingName { get; set; }
        public decimal SettingValue { get; set; }
        public string? PaymentGateway { get; set; }
    }

    public class UpdatePaymentSettingsRequest
    {
        public List<PaymentSettingDto> Settings { get; set; } = new();
    }


    public class RegistrationPaymentRequest
    {
        public string PaymentPaymentId { get; set; } = string.Empty;
        public string PaymentOrderId { get; set; } = string.Empty;
        public string PaymentSignature { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public int PaymentTypeId { get; set; }
    }


    public class ContributionPaymentRequest
    {
        public string ContributionPaymentId { get; set; } = string.Empty;
        public string ContributionOrderId { get; set; } = string.Empty;
        public string ContributionSignature { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }


    public class InitiatePaymentRequest
    {
        public decimal Amount { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Zipcode { get; set; }
        public int PaymentTypeId { get; set; }
        public string? PayOpt { get; set; }
        public string Txnid { get; set; } = string.Empty;
        public int? ContributionId { get; set; }
    }

  
    public class WalletBalanceDto
    {
        public decimal Unit { get; set; }
        public decimal District { get; set; }
        public decimal State { get; set; }
        public decimal Karuthal { get; set; }
    }

    // Payment Response
    public class PaymentResponse
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}