using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Nominee")]
    public class Nominee
    {
        [Key]
        [Column("nomineeId")]
        public int NomineeId { get; set; }

        [Column("nomineeMemberId")]
        public int NomineeMemberId { get; set; }

        [Column("nomineeName")]
        public string? NomineeName { get; set; }

        [Column("nomineeAddress")]
        public string? NomineeAddress { get; set; }

        [Column("nomineeEmail")]
        public string? NomineeEmail { get; set; }

        [Column("nomineeMobilenumber")]
        public string? NomineeMobilenumber { get; set; }

        [Column("nomineeIdProof")]
        public string? NomineeIdProof { get; set; }

        [Column("nomineeIdProofNumber")]
        public string? NomineeIdProofNumber { get; set; }

        [Column("nomineeBankName")]
        public string? NomineeBankName { get; set; }

        [Column("nomineeBankAcName")]
        public string? NomineeBankAcName { get; set; }

        [Column("nomineeBankAcNumber")]
        public string? NomineeBankAcNumber { get; set; }

        [Column("nomineeBankBranch")]
        public string? NomineeBankBranch { get; set; }

        [Column("nomineeIfsc")]
        public string? NomineeIfsc { get; set; }

        [Column("nomineeIdUrl1")]
        public string? NomineeIdUrl1 { get; set; }

        [Column("nomineeIdUrl2")]
        public string? NomineeIdUrl2 { get; set; }

        [Column("nomineeApprovalStatus")]
        public int? NomineeApprovalStatus { get; set; }  // ← nullable int (DB has NULLs)

        [Column("nomineeStatus")]
        public int NomineeStatus { get; set; }            // ← int not bool (DB has 0 and 1)

        [Column("nomineeRelation")]
        public string? NomineeRelation { get; set; }
    }
}