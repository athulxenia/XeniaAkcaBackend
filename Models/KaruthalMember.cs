using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_KaruthalMembers")]
    public class KaruthalMember
    {
        [Key]
        [Column("memberId")]
        public int MemberId { get; set; }

        [Column("memberGroupId")]
        public int MemberGroupId { get; set; }

        [Column("memberParentId")]
        public int? MemberParentId { get; set; }

        [Column("memberDistrictId")]
        public int MemberDistrictId { get; set; }

        [Column("memberUnitId")]
        public int? MemberUnitId { get; set; }

        [Column("memberUserId")]
        public int MemberUserId { get; set; }

        [Column("memberStatus")]
        public int? MemberStatus { get; set; }

        [Column("memberReviseRemarks")]
        public string? MemberReviseRemarks { get; set; }

        [Column("membershipNumberPrefix")]
        public string MembershipNumberPrefix { get; set; } = string.Empty;

        [Column("membershipNumberSuffix")]
        public string? MembershipNumberSuffix { get; set; }

        [Column("membershipNumber")]
        public string MembershipNumber { get; set; } = string.Empty;

        [Column("membershipDate")]
        public DateTime MembershipDate { get; set; }

        [Column("memberActiveStatus")]
        public bool MemberActiveStatus { get; set; }

        [Column("memberName")]
        public string MemberName { get; set; } = string.Empty;

        [Column("memberAddress")]
        public string MemberAddress { get; set; } = string.Empty;

        [Column("memberEmail")]
        public string MemberEmail { get; set; } = string.Empty;

        [Column("memberMobilenumber")]
        public string MemberMobilenumber { get; set; } = string.Empty;

        [Column("memberDob")]
        public DateTime MemberDob { get; set; }

        [Column("memberIdProofNumber")]
        public string? MemberIdProofNumber { get; set; }

        [Column("memberBankName")]
        public string MemberBankName { get; set; } = string.Empty;

        [Column("memberBankAcName")]
        public string MemberBankAcName { get; set; } = string.Empty;

        [Column("memberBankAcNumber")]
        public string MemberBankAcNumber { get; set; } = string.Empty;

        [Column("memberBankBranch")]
        public string MemberBankBranch { get; set; } = string.Empty;

        [Column("memberIfsc")]
        public string MemberIfsc { get; set; } = string.Empty;

        [Column("memberIdUrl1")]
        public string? MemberIdUrl1 { get; set; }

        [Column("memberIdUrl2")]
        public string? MemberIdUrl2 { get; set; }

        [Column("memberBusinessName")]
        public string MemberBusinessName { get; set; } = string.Empty;

        [Column("memberBusinessAddress")]
        public string MemberBusinessAddress { get; set; } = string.Empty;

        [Column("memberAge")]
        public int? MemberAge { get; set; }

        [Column("memberBusinessDetails")]
        public string? MemberBusinessDetails { get; set; }

        [Column("memberBusinessFSSAIno")]
        public string? MemberBusinessFSSAIno { get; set; }

        [Column("memberBusinessCmpyType")]
        public string? MemberBusinessCmpyType { get; set; }

        [Column("memberOldMembership")]
        public string? MemberOldMembership { get; set; }

        [Column("memberGstCertificateUrl")]
        public string? MemberGstCertificateUrl { get; set; }

        [Column("memberPartnershipDeedUrl")]
        public string? MemberPartnershipDeedUrl { get; set; }

        [Column("memberStateWallet")]
        public decimal? MemberStateWallet { get; set; }

        [Column("memberDistrictWallet")]
        public decimal? MemberDistrictWallet { get; set; }

        [Column("memberUnitWallet")]
        public decimal? MemberUnitWallet { get; set; }

        [Column("memberKaruthalWallet")]
        public decimal? MemberKaruthalWallet { get; set; }

        [Column("memberIdProof")]
        public string? MemberIdProof { get; set; }

        [Column("memberGroupName")]
        public string? MemberGroupName { get; set; }
    }
}
