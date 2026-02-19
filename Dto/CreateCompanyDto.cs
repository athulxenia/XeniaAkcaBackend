namespace XeniaTokenBackend.Dto
{
    public class CreateCompanyDto
    {
        public string CompanyName { get; set; }
        public bool Status { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
    }

}
