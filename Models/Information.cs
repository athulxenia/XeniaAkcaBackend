using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Information")]
    public class Information
    {
            [Key]
            [Column("informationId")]
            public int InformationId { get; set; }

            [Column("informationName")]
            public string? InformationName { get; set; }

            [Column("districtId")]
            public int? DistrictId { get; set; }

            [Column("informationImgUrl")]
            public string? InformationImgUrl { get; set; }

            [Column("informationCreatedUser")]
            public string? InformationCreatedUser { get; set; }

            [Column("informationCreatedDate")]
            public DateTime? InformationCreatedDate { get; set; }

            [Column("informationModifiedUser")]
            public string? InformationModifiedUser { get; set; }

            [Column("informationModifiedDate")]
            public DateTime? InformationModifiedDate { get; set; }

            [Column("informationApprovedUser")]       // ← was missing
            public string? InformationApprovedUser { get; set; }

            [Column("informationApprovedDate")]       // ← was missing
            public DateTime? InformationApprovedDate { get; set; }

            [Column("informationContent")]
            public string? InformationContent { get; set; }

            [Column("informationStartDate")]
            public DateTime? InformationStartDate { get; set; }

            [Column("informationEndDate")]
            public DateTime? InformationEndDate { get; set; }

            [Column("informationStatus")]
            public string? InformationStatus { get; set; }

            [Column("activeStatus")]
            public bool ActiveStatus { get; set; }

            [Column("informationApproveStatus")]
            public int? InformationApproveStatus { get; set; }
        }
    }

