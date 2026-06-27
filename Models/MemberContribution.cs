using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_MemberContributions")]
    public class MemberContribution
    {
        [Key]
        [Column("memberContributionId")]
        public int MemberContributionId { get; set; }

        [Column("contributionId")]
        public int ContributionId { get; set; }

        [Column("memberId")]
        public int MemberId { get; set; }

        [Column("contributionAmount")]
        public decimal ContributionAmount { get; set; }

        [Column("paidDate")]
        public DateTime? PaidDate { get; set; }

        [Column("paidBy")]
        public int? PaidBy { get; set; }

        [Column("paidDistrict")]
        public int? PaidDistrict { get; set; }

        [Column("paidUnit")]
        public int? PaidUnit { get; set; }

        [Column("payMode")]
        public string? PayMode { get; set; }

        [Column("paymentStatus")]
        public string? PaymentStatus { get; set; }

        [Column("isCallbackStatus")]
        public int? IsCallbackStatus { get; set; }

        [Column("PaymentTxnRefNo")]
        public string? PaymentTxnRefNo { get; set; }

        [Column("contributionPaymentRef")]       // ← this was missing
        public string? ContributionPaymentRef { get; set; }
        public string ContributionOrderId { get; internal set; }
    }
}