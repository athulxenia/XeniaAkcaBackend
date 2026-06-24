using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaKhraBackend.Models
{
    [Table("AKCA_MemberPayment")]
    public class MemberPayment
    {
        [Key]
        [Column("transactionId")]
        public int TransactionId { get; set; }

        [Column("memberId")]
        public int MemberId { get; set; }  

        [Column("paidAmount")]
        public decimal PaidAmount { get; set; }  

        [Column("paymentTypeId")]
        public int PaymentTypeId { get; set; }   
        [Column("paidDate")]
        public DateTime? PaidDate { get; set; }

        [Column("paidBy")]
        public int PaidBy { get; set; }

        [Column("paidDistrict")]
        public int PaidDistrict { get; set; }

        [Column("paidUnit")]
        public int PaidUnit { get; set; }

        [Column("payMode")]
        public string? PayMode { get; set; }

        [Column("paymentStatus")]
        public string? PaymentStatus { get; set; }

        [Column("isCallbackStatus")]
        public int IsCallbackStatus { get; set; }   
        [Column("PaymentTxnRefNo")]
        public string? PaymentTxnRefNo { get; set; }  

       
        [Column("PaymentPaymentId")]
        public string? PaymentPaymentId { get; set; }

        [Column("PaymentOrderId")]
        public string? PaymentOrderId { get; set; }

        [Column("PaymentSignature")]
        public string? PaymentSignature { get; set; }
    }
}