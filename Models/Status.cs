using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Status")]
    public class Status
    {
        [Key]
        [Column("statusId")]
        public int StatusId { get; set; }

        [Column("status")]
        public string Status1 { get; set; } = string.Empty;
    }
}