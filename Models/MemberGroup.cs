using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_MemberGroups")]
    public class MemberGroup
    {
        [Key]
        [Column("groupId")]
        public int GroupId { get; set; }

        [Column("groupLevel")]
        public string GroupLevel { get; set; } = string.Empty;
    }
}