using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Units")]
    public class Unit
    {
        [Key]
        [Column("unitId")]
        public int UnitId { get; set; }

        [Column("unitName")]
        public string UnitName { get; set; } = string.Empty;

        [Column("unitCode")]
        public string UnitCode { get; set; } = string.Empty;

        [Column("unitDistrictId")]
        public int? UnitDistrictId { get; set; }

        [Column("unitContactPerson")]
        public string UnitContactPerson { get; set; } = string.Empty;

        [Column("unitContactPerson2")]
        public string UnitContactPerson2 { get; set; } = string.Empty;

        [Column("unitContactNumber")]
        public string UnitContactNumber { get; set; } = string.Empty;

        [Column("unitContactNumber2")]
        public string UnitContactNumber2 { get; set; } = string.Empty;

        [Column("unitEmailAddress")]
        public string UnitEmailAddress { get; set; } = string.Empty;

        [Column("unitMemNumberPrefix")]
        public string UnitMemNumberPrefix { get; set; } = string.Empty;

        [Column("lastMembershipNumber")]
        public string LastMembershipNumber { get; set; } = string.Empty;

        [Column("status")]
        public bool Status { get; set; }
    }
}