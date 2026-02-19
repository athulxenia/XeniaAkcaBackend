using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaTokenBackend.Models
{
    [Table("xtm_CompanySubscriptionAddon", Schema = "dbo")]
    public class xtm_CompanySubscriptionAddon
    {
        [Key]
        [Column("subAddonId")]
        public int SubAddonId { get; set; }

        [Column("mainPlanId")]
        public int MainPlanId { get; set; }

        [Column("planId")]
        public int PlanId { get; set; }

        [Column("companyId")]
        public int CompanyId { get; set; }

        [Column("amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column("depCount")]
        public int DepCount { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}