using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_CompanyProfile")]
    public class CompanyProfile
    {
        [Key]
        [Column("companyId")]
        public int CompanyId { get; set; }

        [Column("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [Column("companyAddress")]
        public string CompanyAddress { get; set; } = string.Empty;

        [Column("companyPhone1")]
        public string CompanyPhone1 { get; set; } = string.Empty;

        [Column("companyPhone2")]
        public string CompanyPhone2 { get; set; } = string.Empty;

        [Column("companyRegNo1")]
        public string CompanyRegNo1 { get; set; } = string.Empty;

        [Column("companyRegNo2")]
        public string CompanyRegNo2 { get; set; } = string.Empty;

        [Column("stateCode")]
        public int StateCode { get; set; }

        [Column("stateName")]
        public string StateName { get; set; } = string.Empty;

        [Column("assMemberSchmPrefix")]
        public string? AssMemberSchmPrefix { get; set; }

        [Column("assTermsAndConditionsENG")]
        public string? AssTermsAndConditionsENG { get; set; }

        [Column("assPrivacyPolicyENG")]
        public string? AssPrivacyPolicyENG { get; set; }

        [Column("assTermsAndConditionsML")]
        public string? AssTermsAndConditionsML { get; set; }

        [Column("assPrivacyPolicyML")]
        public string? AssPrivacyPolicyML { get; set; }

        [Column("assTermsAndConditionsTN")]
        public string? AssTermsAndConditionsTN { get; set; }

        [Column("assPrivacyPolicyTN")]
        public string? AssPrivacyPolicyTN { get; set; }

        [Column("assTermsAndConditionsHND")]
        public string? AssTermsAndConditionsHND { get; set; }

        [Column("assPrivacyPolicyHND")]
        public string? AssPrivacyPolicyHND { get; set; }
    }
}