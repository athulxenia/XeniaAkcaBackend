namespace XeniaTokenBackend.Dto
{
    public class DepartmentAppDto
    {
        public int DepID { get; set; }
        public int CompanyID { get; set; }
        public string DepName { get; set; }
        public string? DepPrefix { get; set; }
        public string DepExpire { get; set; }
        public int MaxToken { get; set; }
        public int printTokenValue { get; set; }
        public bool Status { get; set; }
        public bool isService { get; set; }
        public List<ServiceDto> services { get; set; } = new();
    }


    public class ServiceDto
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
    }

}
