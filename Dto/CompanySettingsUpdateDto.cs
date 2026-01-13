namespace XeniaTokenBackend.Dto
{
    public class CompanySettingsUpdateDto
    {
        public int CompanyId { get; set; }
        public bool CollectCustomerName { get; set; }
        public bool PrintCustomerName { get; set; }
        public bool CollectCustomerMobileNumber { get; set; }
        public bool PrintCustomerMobileNumber { get; set; }
        public bool IsCustomCall { get; set; }
        public bool IsServiceEnable { get; set; }
    }

}
