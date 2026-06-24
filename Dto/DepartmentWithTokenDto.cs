namespace XeniaAkcaBackend.Dto
{
    public class DepartmentWithTokenDto
    {
        public int DepID { get; set; }
        public int CompanyID { get; set; }
        public string DepName { get; set; }
        public string? DepPrefix { get; set; }
        public DateTime DepExpire { get; set; }
        public bool Status { get; set; }
        public int? MaximumToken { get; set; }
    }

}
