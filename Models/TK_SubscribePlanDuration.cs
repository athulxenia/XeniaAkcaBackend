using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XeniaTokenBackend.Models;

namespace XeniaQLaunchBackend.Models
{
    [Table("xtm_SubscribePlanDuration", Schema = "dbo")]
    public class xtm_SubscribePlanDuration
    {
        [Key]
        public int PlanDurationId { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        public int DurationDays { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [ForeignKey(nameof(PlanId))]
        public virtual xtm_SubscribePlanDuration? SubscribePlan { get; set; }
    }
}
