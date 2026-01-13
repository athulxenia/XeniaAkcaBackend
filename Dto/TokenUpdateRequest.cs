namespace XeniaTokenBackend.Dto
{
    public class TokenUpdateRequest
    {
        public int CompanyId { get; set; }
        public int DepId { get; set; }
        public required string DepPrefix { get; set; }
        public int CounterId { get; set; }
        public int Status { get; set; } 
        public int? TokenValue { get; set; } 
    }

    public class TokenUpdateResponse
    {
        public object UpdatedCurrentToken { get; set; }
        public object OnCallToken { get; set; }
    }

}
