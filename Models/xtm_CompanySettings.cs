using System.ComponentModel.DataAnnotations;

namespace XeniaTokenBackend.Models
{
    public class xtm_CompanySettings
    {
        [Key]
        public int CompSettingID { get; set; }

        public int CompanyID { get; set; }

        public bool CollectCustomerName { get; set; }

        public bool PrintCustomerName { get; set; }

        public bool CollectCustomerMobileNumber { get; set; }

        public bool PrintCustomerMobileNumber { get; set; }

        public bool IsCustomCall { get; set; }

        public bool IsServiceEnable { get; set; }
        public bool ShowLastCompletedToken { get; set; }
    }
}
