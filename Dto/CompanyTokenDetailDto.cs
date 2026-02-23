namespace XeniaTokenBackend.Dto
{
    public class CompanyTokenListDto
    {
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool Status { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }

    public class CompanyTokenSettingsDto
    {
        public bool CollectCustomerName { get; set; }
        public bool PrintCustomerName { get; set; }
        public bool CollectCustomerMobileNumber { get; set; }
        public bool PrintCustomerMobileNumber { get; set; }
        public bool IsCustomCall { get; set; }
        public bool IsServiceEnable { get; set; }
    }
}
