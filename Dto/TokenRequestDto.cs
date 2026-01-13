namespace XeniaTokenBackend.Dto
{
    public class TokenRequestDto
    {
        public int CompanyID { get; set; }
        public int DepID { get; set; }
        public required string DepPrefix { get; set; }
        public int CounterID { get; set; }
        public int CreatedUserID { get; set; }
        public int Status { get; set; }
        public required string TokenStatus { get; set; }
        public int StatusModifiedUser { get; set; }
        public int NewPrintValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
