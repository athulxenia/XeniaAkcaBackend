namespace XeniaTokenBackend.Dto
{
    public class TokenUpsertDto
    {
        public int CompanyID { get; set; }
        public int DepID { get; set; }
        public string? DepPrefix { get; set; }
        public int? ServiceID { get; set; }
        public int? CounterID { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobileNumber { get; set; }
        public int CreatedUserID { get; set; }
        public string CreatedSource { get; set; } = string.Empty;
        public string TokenStatus { get; set; } = "Pending";
        public int StatusModifiedUser { get; set; }
    }

}
