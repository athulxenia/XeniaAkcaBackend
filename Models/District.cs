using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Districts")]
    public class District
    {
        [Key]
        [Column("districtId")]
        public int DistrictId { get; set; }

        [Column("districtName")]
        public string DistrictName { get; set; } = string.Empty;

        [Column("districtLevelName")]
        public string DistrictLevelName { get; set; } = string.Empty;

        [Column("contactPerson1")]
        public string ContactPerson1 { get; set; } = string.Empty;

        [Column("contactNumber1")]
        public string ContactNumber1 { get; set; } = string.Empty;

        [Column("emailAddress1")]
        public string EmailAddress1 { get; set; } = string.Empty;

        [Column("contactPerson2")]
        public string ContactPerson2 { get; set; } = string.Empty;

        [Column("contactNumber2")]
        public string ContactNumber2 { get; set; } = string.Empty;

        [Column("emailAddress2")]
        public string EmailAddress2 { get; set; } = string.Empty;

        [Column("districtMemberSchmPrefix")]
        public string? DistrictMemberSchmPrefix { get; set; }

        [Column("Status")]
        public bool Status { get; set; }
    }
}