using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_MemberWallet")]
    public class MemberWallet
    {
        [Key]
        [Column("walletId")]
        public int WalletId { get; set; }

        [Column("walletAmount")]
        public decimal? WalletAmount { get; set; }

        [Column("walletMemberId")]
        public int? WalletMemberId { get; set; }

        [Column("walletDate")]
        public DateTime? WalletDate { get; set; }

        [Column("walletTransaction")]
        public string? WalletTransaction { get; set; }

        [Column("walletPurpose")]
        public string? WalletPurpose { get; set; }

        [Column("walletType")]
        public int? WalletType { get; set; }
    }
}