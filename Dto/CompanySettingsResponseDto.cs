using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Dto
{
    public class CompanySettingsResponseDto
    {
        public string status { get; set; }
        public List<DepartmentSettingsDto> DepartmentSettings { get; set; }
        public CompanySettingsDto companySettings { get; set; }
    }

    public class DepartmentSettingsDto
    {
        public int DepID { get; set; }
        public string DepName { get; set; }
        public string DepPrefix { get; set; }
        public string DepExpire { get; set; }   // STRING (ISO)
        public int maxToken { get; set; }
        public int printToken { get; set; }
        public bool isService { get; set; }
        public List<ServiceSettingsDto> services { get; set; }
    }

    public class ServiceSettingsDto
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
    }

    public class CompanySettingsDto
    {
        public int CompSettingID { get; set; }
        public int CompanyID { get; set; }
        public bool CollectCustomerName { get; set; }
        public bool PrintCustomerName { get; set; }
        public bool CollectCustomerMobileNumber { get; set; }
        public bool PrintCustomerMobileNumber { get; set; }
        public bool IsCustomCall { get; set; }
        public bool IsServiceEnable { get; set; }
        public bool hasExpiredDepartments { get; set; }
    }


}
