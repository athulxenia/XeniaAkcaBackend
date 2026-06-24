// Dto/ContributionPaymentRequest.cs
namespace XeniaKhraBackend.Dto
{
    public class ContributionPaymentRequest
    {
        public string ContributionPaymentId { get; set; }
        public string ContributionOrderId { get; set; }
        public string ContributionSignature { get; set; }
        public string PaymentStatus { get; set; }
    }

    public class CreateOrderRequest
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int PaymentTypeId { get; set; }
        public int PaidBy { get; set; }
        public int PaidDistrict { get; set; }
        public int PaidUnit { get; set; }
        public string PayMode { get; set; }
        public int? ContributionId { get; set; }
    }

    public class CreateOrderResponse
    {
        public string OrderId { get; set; }
        public decimal PayAmount { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool? OrderidStatus { get; set; }
    }

    public class PaymentData
    {
        public decimal Amount { get; set; }
        public int PaymentTypeId { get; set; }
        public int PaidBy { get; set; }
        public int PaidDistrict { get; set; }
        public int PaidUnit { get; set; }
        public string PayMode { get; set; }
        public int UserId { get; set; }
        public int? ContributionId { get; set; }
        public string OrderId { get; set; }
        public string PaymentOrderId { get; set; }
    }

    public class PaymentResponse
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public int Amount { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }
        public string ErrorDescription { get; set; }
        public long CreatedAt { get; set; }
    }

    public class RefundResponse
    {
        public string Id { get; set; }
        public string PaymentId { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }
        public long CreatedAt { get; set; }
    }
}
