using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaTokenBackend.Models
{
    [Table("xtm_SubscribePlan", Schema = "dbo")]
    public class xtm_SubscribePlan
    {
        [Key]
        [Column("planId")]
        public int PlanId { get; set; }

        [Column("companyId")]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("planName")]
        public string PlanName { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("planDescription")]
        public string? PlanDescription { get; set; }

        [Column("planDep")]
        public int PlanDep { get; set; }

        [Column("planIsAddOn")]
        public bool PlanIsAddOn { get; set; }

        [Column("planCreatedBy")]
        public int? PlanCreatedBy { get; set; }

        [Column("planCreatedOn")]
        public DateTime PlanCreatedOn { get; set; }

        [Column("planModifiedBy")]
        public int? PlanModifiedBy { get; set; }

        [Column("planModifiedOn")]
        public DateTime? PlanModifiedOn { get; set; }

        [Column("planActive")]
        public bool PlanActive { get; set; }
    }
}
