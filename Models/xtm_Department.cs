using System.ComponentModel.DataAnnotations;

namespace XeniaTokenBackend.Models
{
    public class xtm_Department
    {
        [Key]
        public int DepID { get; set; }
        public int CompanyID { get; set; }
        public required string DepName { get; set; }
        public string? DepPrefix { get; set; }
        public bool Status { get; set; }
    }
}
