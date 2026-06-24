using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaTokenBackend.Models
{
    [Table("AKCA_Server")]
    public class Server
    {
        [Key]
        [Column("serverID")]
        public int ServerId { get; set; }

        [Column("iosAppVersion")]
        public string? IosAppVersion { get; set; }

        [Column("appVersion")]
        public string AppVersion { get; set; } = string.Empty;

        [Column("serverupdate")]
        public bool ServerUpdate { get; set; }
    }
}