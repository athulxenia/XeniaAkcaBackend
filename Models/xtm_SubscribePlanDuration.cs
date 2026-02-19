using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaTokenBackend.Models
{
    [Table("xtm_SubscribePlanDuration", Schema = "dbo")]
    public class xtm_SubscribePlanDuration
    {
        [Key]
        [Column("planDurationId")]
        public int PlanDurationId { get; set; }

        [Column("planId")]
        public int PlanId { get; set; }

        [Column("durationDays")]
        public int DurationDays { get; set; }

        [Column("price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; }

        [Column("createdOn")]
        public DateTime CreatedOn { get; set; }

        [Column("modifiedOn")]
        public DateTime? ModifiedOn { get; set; }

    }
}
