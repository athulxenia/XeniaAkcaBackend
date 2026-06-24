using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Settings")]
    public class Setting
    {
        [Key]
        [Column("settingId")]
        public int SettingId { get; set; }

        [Column("settingName")]
        public string? SettingName { get; set; }

        [Column("settingValue")]
        public decimal? SettingValue { get; set; }

        [Column("paymentGateway")]
        public string? PaymentGateway { get; set; }

        [Column("settingRemark")]
        public string? SettingRemark { get; set; }
    }
}