using System.ComponentModel.DataAnnotations;

namespace XeniaTokenBackend.Models
{
    public class xtm_AppSettings
    {
        [Key]
        public int AppID { get; set; }
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public string AppVersionMandatory { get; set; }
    }

}
