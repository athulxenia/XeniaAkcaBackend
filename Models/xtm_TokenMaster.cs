using System.ComponentModel.DataAnnotations;

namespace XeniaTokenBackend.Models
{
    public class xtm_TokenMaster
    {
        [Key]
        public int TokenMasterID { get; set; }

        public int CompanyID { get; set; }

        public int DepID { get; set; }

        public int PrintTokenValue { get; set; }

        public int TriggerValue { get; set; }

        public int MaximumToken { get; set; }
    }
}
