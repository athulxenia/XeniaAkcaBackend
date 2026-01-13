namespace XeniaTokenBackend.Dto
{
    public class TokenRecallDto
    {
        public int TokenId { get; set; }
        public int TokenValue { get; set; }
        public int CompanyId { get; set; }
        public string DepPrefix { get; set; }
        public int DepId { get; set; }
        public int ServiceId { get; set; }
        public int CounterId { get; set; }
        public int UserId { get; set; }
    }
}
