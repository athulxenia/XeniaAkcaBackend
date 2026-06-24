using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Contributions")]
    public class Contribution
    {
        [Key]
        [Column("contributionId")]
        public int ContributionId { get; set; }

        [Column("contributionMemberId")]
        public int ContributionMemberId { get; set; }

        [Column("contributionImgUrl")]
        public string? ContributionImgUrl { get; set; }

        [Column("contributionText")]
        public string? ContributionText { get; set; }

        [Column("contributionContent")]
        public string? ContributionContent { get; set; }

        [Column("contributionAmount")]
        public decimal ContributionAmount { get; set; }

        [Column("contributionInitiatedDate")]
        public DateTime ContributionInitiatedDate { get; set; }

        [Column("contributionDueDate")]
        public DateTime ContributionDueDate { get; set; }

        [Column("contributionStatus")]
        public string? ContributionStatus { get; set; }

        [Column("activeStatus")]
        public bool ActiveStatus { get; set; }             

        [Column("contributionApprovalStatus")]
        public int? ContributionApprovalStatus { get; set; }

        [Column("contributionType")]
        public string? ContributionType { get; set; }

        [Column("contributionChequeNo")]
        public string? ContributionChequeNo { get; set; }

        [Column("contributionHandOveredTo")]
        public string? ContributionHandOveredTo { get; set; }

        [Column("contributionRemarks")]
        public string? ContributionRemarks { get; set; }
    }

}


  


