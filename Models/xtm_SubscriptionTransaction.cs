using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaTokenBackend.Models
{
    [Table("xtm_SubscriptionTransaction", Schema = "dbo")]
    public class xtm_SubscriptionTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int CompanyId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentProvider { get; set; }
        public string? TransactionRefId { get; set; }
        public string ProviderTransactionId { get; set; }
        public string? PaymentLink { get; set; }
        public string Status { get; set; } = "INITIATED";
        public DateTime CreatedOn { get; set; }
    }
}