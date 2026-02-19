namespace XeniaTokenBackend.Dto
{
    public class CreateDepartmentDto
    {
        public int CompanyID { get; set; }

        public string DepName { get; set; } = string.Empty;

        public string DepPrefix { get; set; } = string.Empty;

        public bool Status { get; set; }

        public int MaximumToken { get; set; }
    }
}
